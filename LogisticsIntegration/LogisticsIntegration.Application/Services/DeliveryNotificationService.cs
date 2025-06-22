using LogisticsIntegration.ApiClients.BCompany.Interfaces;
using LogisticsIntegration.Domain.Dtos;
using LogisticsIntegration.Domain.Entities;
using LogisticsIntegration.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogisticsIntegration.Application.Services
{
    public class DeliveryNotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICustomerDeliveryApiClient _deliveryApiClient;
        private readonly ILogger<DeliveryNotificationService> _logger;

        public DeliveryNotificationService(IUnitOfWork unitOfWork, ICustomerDeliveryApiClient deliveryApiClient, ILogger<DeliveryNotificationService> logger)
        {
            _unitOfWork = unitOfWork;
            _deliveryApiClient = deliveryApiClient;
            _logger = logger;
        }

        public async Task NotifyPendingDeliveriesAsync()
        {
            _logger.LogInformation("Teslimat bildirim işlemi başlatıldı.");

            try
            {
                // Bildirilmemiş teslimatları çekiyoruz.
                var pendingDeliveries = await _unitOfWork.Deliveries
                    .FindAsync(d => d.IsNotifiedToCustomer == false && d.OrderId != 0); // OrderId'nin geçerli olduğundan emin olalım

                var deliveriesToProcess = new List<Delivery>();
                foreach (var delivery in pendingDeliveries)
                {
                    var order = await _unitOfWork.Orders.GetByIdAsync(delivery.OrderId);
                    if (order != null && order.Status == OrderStatus.Delivered) // Sadece 'Teslim Edildi' statüsündeki siparişler için bildirim yap
                    {
                        delivery.Order = order; 
                        deliveriesToProcess.Add(delivery);
                    }
                }

                if (!deliveriesToProcess.Any())
                {
                    _logger.LogInformation("Bildirim bekleyen teslimat bulunamadı.");
                    return;
                }

                foreach (var delivery in deliveriesToProcess)
                {
                    var order = delivery.Order; // Artık order'a direkt erişebiliriz

                    _logger.LogInformation($"Sipariş ID {order.CustomerOrderId} için teslimat bildirimi hazırlanıyor.");

                    var notificationDto = new CustomerDeliveryNotificationDTO
                    {
                       
                        OrderId = order.CustomerOrderId, 
                        DeliveryDate = delivery.DeliveryDate,
                        PlateNumber = delivery.PlateNumber,
                        DelivererName = delivery.DelivererName
                    };

                    bool success = await _deliveryApiClient.NotifyCustomerDeliveryAsync(notificationDto);

                    if (success)
                    {
                        // Bildirim başarılı olduysa Delivery kaydını güncelle
                        delivery.IsNotifiedToCustomer = true;
                        delivery.NotificationDate = DateTime.UtcNow;
                        _unitOfWork.Deliveries.Update(delivery);

                        // Sipariş statüsünü "2-Tamamlandı" olarak güncelle
                        order.Status = OrderStatus.Completed; // Siparişin statüsünü güncelle
                        _unitOfWork.Orders.Update(order);

                        await _unitOfWork.CompleteAsync(); // Hem Delivery hem de Order güncellemesini kaydet
                        _logger.LogInformation($"Sipariş ID {order.Id} (Müşteri Sipariş ID: {order.CustomerOrderId}) teslimat bildirimi yapıldı ve statüsü 'Tamamlandı' olarak güncellendi.");
                    }
                    else
                    {
                        _logger.LogError($"Sipariş ID {order.Id} (Müşteri Sipariş ID: {order.CustomerOrderId}) için teslimat bildirimi BAŞARISIZ OLDU. Tekrar denenecek.");
                        // Başarısız olursa IsNotifiedToCustomer false kalacak, bir sonraki döngüde tekrar denenecek.
                    }
                }
                _logger.LogInformation("Teslimat bildirim işlemi tamamlandı.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Teslimat bildirim işlemi sırasında bir hata oluştu.");
                // throw; 
            }
        }
    }
}
