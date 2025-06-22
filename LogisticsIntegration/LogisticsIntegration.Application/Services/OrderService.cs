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
    public class OrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrderService> _logger;

        public OrderService(IUnitOfWork unitOfWork, ILogger<OrderService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            // Tüm siparişleri ve ürünlerini dahil ederek getir
            var orders = await _unitOfWork.Orders.GetAllAsync();
            return orders; 
        }

        public async Task<Order> GetOrderByIdAsync(int id)
        {
            return await _unitOfWork.Orders.GetByIdAsync(id);
        }

        public async Task UpdateOrderStatusAndDeliveryInfoAsync(int orderId, OrderStatus newStatus, string plateNumber, string delivererName)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning($"Sipariş ID {orderId} bulunamadı.");
                throw new ArgumentException("Sipariş bulunamadı.");
            }

            if (order.Status == OrderStatus.Completed)
            {
                _logger.LogWarning($"Sipariş ID {orderId} zaten tamamlanmış, durumu değiştirilemez.");
                throw new InvalidOperationException("Tamamlanmış siparişin durumu değiştirilemez.");
            }

            order.Status = newStatus;
            _unitOfWork.Orders.Update(order);

            if (newStatus == OrderStatus.Delivered)
            {
                // Mevcut teslimat kaydı var mı kontrol et, yoksa yeni oluştur
                var existingDelivery = (await _unitOfWork.Deliveries.FindAsync(d => d.OrderId == order.Id)).FirstOrDefault();

                if (existingDelivery == null)
                {
                    var delivery = new Delivery
                    {
                        OrderId = order.Id,
                        DeliveryDate = DateTime.Now,
                        PlateNumber = plateNumber,
                        DelivererName = delivererName,
                        IsNotifiedToCustomer = false
                    };
                    await _unitOfWork.Deliveries.AddAsync(delivery);
                }
                else
                {
                    // Eğer mevcut teslimat varsa ve henüz bildirilmediyse güncelle
                    existingDelivery.DeliveryDate = DateTime.Now;
                    existingDelivery.PlateNumber = plateNumber;
                    existingDelivery.DelivererName = delivererName;
                    existingDelivery.IsNotifiedToCustomer = false; // Yeniden teslim edildi sayıldığı için false yap
                    _unitOfWork.Deliveries.Update(existingDelivery);
                }
            }

            await _unitOfWork.CompleteAsync();
            _logger.LogInformation($"Sipariş ID {orderId} durumu '{newStatus}' olarak güncellendi.");
        }
    }
}
