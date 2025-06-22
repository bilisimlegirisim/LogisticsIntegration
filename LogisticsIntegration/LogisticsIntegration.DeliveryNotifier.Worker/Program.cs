using LogisticsIntegration.ApiClients.BCompany.Implementations.Mocks;
using LogisticsIntegration.ApiClients.BCompany.Interfaces;
using LogisticsIntegration.DeliveryNotifier.Worker;
using LogisticsIntegration.Domain.Interfaces;
using LogisticsIntegration.Infrastructure.Data;
using LogisticsIntegration.Infrastructure.Repositories;
using LogisticsIntegration.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace LogisticsIntegration.DeliveryNotifier.Worker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
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
                    // DbContext Kaydý
                    var connectionString = hostContext.Configuration.GetConnectionString("DefaultConnection");
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlite(connectionString)); 

                    // Repository ve UnitOfWork Kaydý
                    services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
                    services.AddScoped<IOrderRepository, OrderRepository>();
                    services.AddScoped<IUnitOfWork, UnitOfWork>();

                    // API Clients Kaydý (Mock implementasyonlarý)
                    services.AddScoped<ICustomerOrderSoapClient, MockCustomerOrderSoapClient>();
                    services.AddScoped<ICustomerDeliveryApiClient, MockCustomerDeliveryApiClient>();

                    // Worker Hizmetini HostedService olarak kaydet
                    services.AddHostedService<Worker>();
                });
    }
}
