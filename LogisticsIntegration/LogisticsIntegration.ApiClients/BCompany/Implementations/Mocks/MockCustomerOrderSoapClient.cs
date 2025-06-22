using LogisticsIntegration.ApiClients.BCompany.Interfaces;
using LogisticsIntegration.Domain.Dtos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogisticsIntegration.ApiClients.BCompany.Implementations.Mocks
{
    public class MockCustomerOrderSoapClient : ICustomerOrderSoapClient
    {
        private readonly ILogger<MockCustomerOrderSoapClient> _logger;

        public MockCustomerOrderSoapClient(ILogger<MockCustomerOrderSoapClient> logger)
        {
            _logger = logger;
        }

        public async Task<List<CustomerOrderDto>> GetOrdersForPreviousDayAsync(DateTime referenceDate)
        {
            _logger.LogInformation($"[MOCK] B Şirketi SOAP Servisinden {referenceDate.AddDays(-1):yyyy-MM-dd} tarihli siparişler çekiliyor...");

            var orders = new List<CustomerOrderDto>
            {
                new CustomerOrderDto
                {
                    CustomerOrderId = "BCO-001",
                    OrderDate = referenceDate.AddDays(-1),
                    ShippingPoint = "B Depo 1",
                    RecipientName = "Halil Türk",
                    ContactPhone = "05554445555",
                    Items = new List<CustomerOrderItemDto>
                    {
                        new CustomerOrderItemDto { ProductName = "Ürün A", Quantity = 2 },
                        new CustomerOrderItemDto { ProductName = "Ürün B", Quantity = 1 }
                    }
                },
                new CustomerOrderDto
                {
                    CustomerOrderId = "BCO-002",
                    OrderDate = referenceDate.AddDays(-1),
                    ShippingPoint = "B Depo 2",
                    RecipientName = "Ayşe Yılmaz",
                    ContactPhone = "05554445555",
                    Items = new List<CustomerOrderItemDto>
                    {
                        new CustomerOrderItemDto { ProductName = "Ürün C", Quantity = 5 }
                    }
                }
            };

            await Task.Delay(100);
            return orders;
        }
    }
}
