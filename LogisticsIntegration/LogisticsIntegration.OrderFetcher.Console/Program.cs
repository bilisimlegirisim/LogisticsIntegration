using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;
using LogisticsIntegration.Infrastructure.Data;
using LogisticsIntegration.Infrastructure.UnitOfWork;
using LogisticsIntegration.Domain.Interfaces;
using LogisticsIntegration.Infrastructure.Repositories; 

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
                await dbContext.Database.MigrateAsync(); // Uygulama başladığında otomatik migration uygular
                Console.WriteLine("Veritabanı migration'ları uygulandı veya güncel.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Veritabanı migration'ları uygulanırken bir hata oluştu: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
        // Konsolun hemen kapanmaması için 
        Console.WriteLine("Uygulama işini bitirdi. Kapatmak için herhangi bir tuşa basın...");
        Console.ReadKey();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                // appsettings.json dosyasını doğru yerden okumasını sağlar
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddEnvironmentVariables();
            })
            .ConfigureServices((hostContext, services) =>
            {
                var connectionString = hostContext.Configuration.GetConnectionString("DefaultConnection");
                services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(connectionString));

                services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>)); 
                services.AddScoped<IOrderRepository, OrderRepository>(); 
                services.AddScoped<IUnitOfWork, UnitOfWork>(); 

            });
}