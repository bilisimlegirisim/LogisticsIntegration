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
                            _logger.LogInformation($"Sipariþ ID {order.Id} için teslimat bildirimi gönderiliyor...");

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
                                    _logger.LogInformation($"Sipariþ ID {order.Id} için teslimat bildirimi baþarýlý ve güncellendi.");
                                }
                            }
                            else
                            {
                                _logger.LogWarning($"Sipariþ ID {order.Id} için teslimat bildirimi baþarýsýz.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Worker içerisinde bir hata oluþtu.");
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            _logger.LogInformation("Delivery Notifier Worker stopped at: {time}", DateTimeOffset.Now);
        }
    }
}
