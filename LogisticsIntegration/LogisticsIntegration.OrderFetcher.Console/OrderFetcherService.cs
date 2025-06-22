using LogisticsIntegration.ApiClients.BCompany.Interfaces;
using LogisticsIntegration.Domain.Entities;
using LogisticsIntegration.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogisticsIntegration.OrderFetcher.Console
{
    public class OrderFetcherService
    {
        private readonly ICustomerOrderSoapClient _customerOrderSoapClient;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrderFetcherService> _logger;

        public OrderFetcherService(
            ICustomerOrderSoapClient customerOrderSoapClient,
            IUnitOfWork unitOfWork,
            ILogger<OrderFetcherService> logger)
        {
            _customerOrderSoapClient = customerOrderSoapClient;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task FetchAndProcessOrdersAsync()
        {
            _logger.LogInformation("Siparişler dış API'den çekiliyor...");

            try
            {

                var mockOrders = await _customerOrderSoapClient.GetOrdersForPreviousDayAsync(DateTime.Now);

                if (mockOrders == null || !mockOrders.Any())
                {
                    _logger.LogInformation("Dış API'den çekilecek yeni sipariş bulunamadı.");
                    return;
                }

                _logger.LogInformation($"{mockOrders.Count()} adet sipariş çekildi. Veritabanına kaydediliyor...");

                foreach (var mockOrder in mockOrders)
                {
                    var order = new Order
                    {
                        CustomerOrderId = mockOrder.CustomerOrderId,
                        OrderDate = mockOrder.OrderDate,
                        ShippingPoint = mockOrder.ShippingPoint,
                        RecipientName = mockOrder.RecipientName,
                        ContactPhone = mockOrder.ContactPhone,
                        Status = OrderStatus.Pending,
                        OrderItems = new List<OrderItem>()
                    };

                    foreach (var mockOrderItem in mockOrder.Items) 
                    {
                        order.OrderItems.Add(new OrderItem
                        {
                            ProductName = mockOrderItem.ProductName,
                            Quantity = mockOrderItem.Quantity
                        });
                    }

                    await _unitOfWork.Orders.AddAsync(order);
                    _logger.LogInformation($"Sipariş ID: {order.CustomerOrderId} veritabanına eklendi.");
                }

                await _unitOfWork.CompleteAsync();
                _logger.LogInformation("Tüm çekilen siparişler veritabanına başarıyla kaydedildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş çekme ve işleme sırasında bir hata oluştu.");
            }
        }
    }
}
