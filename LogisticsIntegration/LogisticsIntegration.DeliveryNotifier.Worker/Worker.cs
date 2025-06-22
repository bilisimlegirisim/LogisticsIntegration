using LogisticsIntegration.ApiClients.BCompany.Interfaces;
using LogisticsIntegration.Application.Services;
using LogisticsIntegration.Domain.Dtos;
using LogisticsIntegration.Domain.Interfaces;

namespace LogisticsIntegration.DeliveryNotifier.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Delivery Notifier Worker baþlatýldý: {time}", DateTimeOffset.Now);

            // Periyot: Her 5 dakikada bir çalýþacak
            TimeSpan delay = TimeSpan.FromMinutes(5);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker çalýþýyor: {time}", DateTimeOffset.Now);

                // Her döngüde yeni bir servis kapsamý (scope) oluþturulur.
                // Bu, Scoped lifetime'daki servislerin (örn. DbContext, UnitOfWork) doðru þekilde yönetilmesini saðlar.
                using (var scope = _scopeFactory.CreateScope())
                {
                    var deliveryNotificationService = scope.ServiceProvider.GetRequiredService<DeliveryNotificationService>();

                    try
                    {
                        // DeliveryNotificationService'deki iþ mantýðýný çaðýrýyoruz.
                        // Tüm sorgulama, bildirim ve veritabaný güncelleme iþini bu servis yapacak.
                        await deliveryNotificationService.NotifyPendingDeliveriesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Delivery Notifier Worker'da bildirim iþlemi sýrasýnda bir hata oluþtu.");
                        
                    }
                }

             
                await Task.Delay(delay, stoppingToken);
            }

            _logger.LogInformation("Delivery Notifier Worker durduruldu: {time}", DateTimeOffset.Now);
        }
    }
}
