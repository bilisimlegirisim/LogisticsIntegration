using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;
using LogisticsIntegration.Infrastructure.Data;
using LogisticsIntegration.Infrastructure.UnitOfWork;
using LogisticsIntegration.Domain.Interfaces;
using LogisticsIntegration.Infrastructure.Repositories;
using LogisticsIntegration.OrderFetcher.Console;
using LogisticsIntegration.ApiClients.BCompany.Interfaces;
using LogisticsIntegration.ApiClients.BCompany.Implementations.Mocks;
class Program
{
    static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var dbContext = services.GetRequiredService<ApplicationDbContext>();
                await dbContext.Database.MigrateAsync();
                Console.WriteLine("Veritabanı migration'ları uygulandı veya oluşturuldu.");
                Console.WriteLine("OrderFetcherConsole uygulaması başlatılıyor ve siparişler çekiliyor...");
                var fetcherService = services.GetRequiredService<OrderFetcherService>();
                await fetcherService.FetchAndProcessOrdersAsync(); 

                Console.WriteLine("OrderFetcherConsole uygulaması başarıyla çalıştı ve siparişler işlendi.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OrderFetcherConsole başlatılırken bir hata oluştu: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
        
        Console.WriteLine("Uygulama işini bitirdi. Kapatmak için herhangi bir tuşa basın...");
        Console.ReadKey();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddEnvironmentVariables();
            })

            .ConfigureServices((hostContext, services) =>
            {
                // DbContext Kaydı
                var connectionString = hostContext.Configuration.GetConnectionString("DefaultConnection");
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite(connectionString));

                // Repository ve UnitOfWork Kaydı
                services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
                services.AddScoped<IOrderRepository, OrderRepository>();
                services.AddScoped<IUnitOfWork, UnitOfWork>();

                // API Clients Kaydı (Mock implementasyonları)
                services.AddScoped<ICustomerOrderSoapClient, MockCustomerOrderSoapClient>();
                services.AddScoped<ICustomerDeliveryApiClient, MockCustomerDeliveryApiClient>();

                services.AddScoped<OrderFetcherService>();
            });
}