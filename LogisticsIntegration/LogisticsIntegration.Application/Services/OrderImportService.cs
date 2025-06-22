using LogisticsIntegration.ApiClients.BCompany.Interfaces;
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
    public class OrderImportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICustomerOrderSoapClient _customerOrderSoapClient;
        private readonly ILogger<OrderImportService> _logger;

        public OrderImportService(IUnitOfWork unitOfWork, ICustomerOrderSoapClient customerOrderSoapClient, ILogger<OrderImportService> logger)
        {
            _unitOfWork = unitOfWork;
            _customerOrderSoapClient = customerOrderSoapClient;
            _logger = logger;
        }

        public async Task ImportDailyOrdersAsync(DateTime referenceDate)
        {
            _logger.LogInformation($"Sipariş import işlemi başlatıldı. Referans Tarih: {referenceDate.ToShortDateString()}");

            try
            {
                var customerOrders = await _customerOrderSoapClient.GetOrdersForPreviousDayAsync(referenceDate);

                if (customerOrders == null || !customerOrders.Any())
                {
                    _logger.LogInformation("Yeni sipariş bulunamadı.");
                    return;
                }

                foreach (var customerOrderDto in customerOrders)
                {
                    var existingOrder = await _unitOfWork.Orders.FindAsync(o => o.CustomerOrderId == customerOrderDto.CustomerOrderId);

                    if (existingOrder.Any())
                    {
                        _logger.LogWarning($"Sipariş ID {customerOrderDto.CustomerOrderId} zaten mevcut. Güncelleme atlanıyor.");
                        continue;
                    }

                    var order = new Order
                    {
                        CustomerOrderId = customerOrderDto.CustomerOrderId,
                        OrderDate = customerOrderDto.OrderDate,
                        ShippingPoint = customerOrderDto.ShippingPoint,
                        RecipientName = customerOrderDto.RecipientName,
                        ContactPhone = customerOrderDto.ContactPhone,
                        Status = OrderStatus.Pending
                    };

                    order.OrderItems = customerOrderDto.Items.Select(itemDto => new OrderItem
                    {
                        ProductName = itemDto.ProductName,
                        Quantity = itemDto.Quantity
                    }).ToList();

                    await _unitOfWork.Orders.AddAsync(order);
                    _logger.LogInformation($"Sipariş {order.CustomerOrderId} 'Bekliyor' statüsünde eklendi.");
                }

                await _unitOfWork.CompleteAsync();
                _logger.LogInformation($"Toplam {customerOrders.Count} sipariş başarılı şekilde import edildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş import işlemi sırasında bir hata oluştu.");
                throw;
            }
        }
    }
}
