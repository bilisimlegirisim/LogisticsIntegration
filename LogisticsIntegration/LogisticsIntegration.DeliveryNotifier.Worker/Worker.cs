using LogisticsIntegration.ApiClients.BCompany.Interfaces;
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
            _logger.LogInformation("Delivery Notifier Worker started at: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                using (var scope = _scopeFactory.CreateScope())
                {
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var deliveryApiClient = scope.ServiceProvider.GetRequiredService<ICustomerDeliveryApiClient>();

                    try
                    {
                        var ordersToNotify = await unitOfWork.Orders.GetOrdersNotNotifiedToCustomer();

                        foreach (var order in ordersToNotify)
                        {
                            _logger.LogInformation($"Sipari� ID {order.Id} i�in teslimat bildirimi g�nderiliyor...");

                            var isNotified = await deliveryApiClient.NotifyCustomerDeliveryAsync(
                                new CustomerDeliveryNotificationDTO
                                {
                                    OrderId = order.Id,
                                    CustomerEmail = "customer@mail.com", 
                                    DeliveryDate = DateTime.UtcNow
                                });

                            if (isNotified)
                            {
                                var delivery = await unitOfWork.Deliveries.FindAsync(d => d.OrderId == order.Id);
                                if (delivery.Any())
                                {
                                    var currentDelivery = delivery.FirstOrDefault();
                                    currentDelivery.IsNotifiedToCustomer = true;
                                    currentDelivery.NotificationDate = DateTime.UtcNow; 
                                    unitOfWork.Deliveries.Update(currentDelivery);
                                    await unitOfWork.CompleteAsync();
                                    _logger.LogInformation($"Sipari� ID {order.Id} i�in teslimat bildirimi ba�ar�l� ve g�ncellendi.");
                                }
                            }
                            else
                            {
                                _logger.LogWarning($"Sipari� ID {order.Id} i�in teslimat bildirimi ba�ar�s�z.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Worker i�erisinde bir hata olu�tu.");
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            _logger.LogInformation("Delivery Notifier Worker stopped at: {time}", DateTimeOffset.Now);
        }
    }
}
