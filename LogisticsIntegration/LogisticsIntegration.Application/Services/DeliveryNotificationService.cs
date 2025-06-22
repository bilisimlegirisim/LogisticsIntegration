using LogisticsIntegration.ApiClients.BCompany.Interfaces;
using LogisticsIntegration.Domain.Dtos;
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

                var ordersToProcess = await _unitOfWork.Orders.GetOrdersNotNotifiedToCustomer(); // Order listesi döndürür.

                if (ordersToProcess == null || !ordersToProcess.Any())
                {
                    _logger.LogInformation("Bildirilecek yeni teslimat bulunamadı.");
                    return;
                }

                foreach (var order in ordersToProcess) // Her bir Order için işlem yap
                {
                    // İlgili Order'ın henüz bildirim yapılmamış Delivery'sini bul
                    var delivery = (await _unitOfWork.Deliveries.FindAsync(d => d.OrderId == order.Id && !d.IsNotifiedToCustomer))
                                         .FirstOrDefault();

                    if (delivery == null)
                    {
                        _logger.LogWarning($"Sipariş ID {order.Id} için bekleyen ve henüz bildirilmemiş teslimat kaydı bulunamadı. Atlanıyor.");
                        continue;
                    }

                    _logger.LogInformation($"Sipariş ID {order.Id} (Müşteri Sipariş ID: {order.CustomerOrderId}) için teslimat bildirimi hazırlanıyor.");

                
                    var notificationDto = new CustomerDeliveryNotificationDTO
                    {
                        OrderId = order.Id, 
                        CustomerEmail = "customer@mail.com", 
                        DeliveryDate = delivery.DeliveryDate
                    };

                    
                    bool notificationSuccess = await _deliveryApiClient.NotifyCustomerDeliveryAsync(notificationDto);

                    if (notificationSuccess)
                    {
                        delivery.IsNotifiedToCustomer = true;
                        delivery.NotificationDate = DateTime.UtcNow; // Delivery nesnesindeki yeni alan

                        // _unitOfWork.Orders.Update(order); // Sipariş durumunu değiştirmiyorsak buna gerek yok.
                        _unitOfWork.Deliveries.Update(delivery);

                        await _unitOfWork.CompleteAsync();
                        _logger.LogInformation($"Sipariş ID {order.Id} (Müşteri Sipariş ID: {order.CustomerOrderId}) teslimat bildirimi yapıldı ve Delivery kaydı güncellendi.");
                    }
                    else
                    {
                        _logger.LogError($"Sipariş ID {order.Id} (Müşteri Sipariş ID: {order.CustomerOrderId}) için teslimat bildirimi BAŞARISIZ OLDU. Tekrar denenecek.");
                    }
                }
                _logger.LogInformation("Teslimat bildirim işlemi tamamlandı.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Teslimat bildirim işlemi sırasında bir hata oluştu.");
                throw; 
            }
        }
    }
}
