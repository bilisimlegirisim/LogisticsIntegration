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
            _logger.LogInformation("Delivery Notifier Worker ba�lat�ld�: {time}", DateTimeOffset.Now);

            // Periyot: Her 5 dakikada bir �al��acak
            TimeSpan delay = TimeSpan.FromMinutes(5);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker �al���yor: {time}", DateTimeOffset.Now);

                // Her d�ng�de yeni bir servis kapsam� (scope) olu�turulur.
                // Bu, Scoped lifetime'daki servislerin (�rn. DbContext, UnitOfWork) do�ru �ekilde y�netilmesini sa�lar.
                using (var scope = _scopeFactory.CreateScope())
                {
                    var deliveryNotificationService = scope.ServiceProvider.GetRequiredService<DeliveryNotificationService>();

                    try
                    {
                        // DeliveryNotificationService'deki i� mant���n� �a��r�yoruz.
                        // T�m sorgulama, bildirim ve veritaban� g�ncelleme i�ini bu servis yapacak.
                        await deliveryNotificationService.NotifyPendingDeliveriesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Delivery Notifier Worker'da bildirim i�lemi s�ras�nda bir hata olu�tu.");
                        
                    }
                }

             
                await Task.Delay(delay, stoppingToken);
            }

            _logger.LogInformation("Delivery Notifier Worker durduruldu: {time}", DateTimeOffset.Now);
        }
    }
}
