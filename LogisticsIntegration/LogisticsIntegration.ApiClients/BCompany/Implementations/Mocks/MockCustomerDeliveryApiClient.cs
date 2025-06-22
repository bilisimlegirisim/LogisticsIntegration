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
    public class MockCustomerDeliveryApiClient : ICustomerDeliveryApiClient
    {
        private readonly ILogger<MockCustomerDeliveryApiClient> _logger;

        public MockCustomerDeliveryApiClient(ILogger<MockCustomerDeliveryApiClient> logger)
        {
            _logger = logger;
        }

        
        public async Task<bool> NotifyCustomerDeliveryAsync(CustomerDeliveryNotificationDTO notification) 
        {
            
            _logger.LogInformation($"[MOCK] B Şirketi Teslimat API'sine bildirim gönderiliyor: Sipariş ID: {notification.OrderId}");

            bool success = new Random().Next(0, 100) < 90;

            await Task.Delay(200);

            if (success)
            {
                _logger.LogInformation($"[MOCK] Teslimat bildirimi başarılı: Sipariş ID: {notification.OrderId}");
                return true;
            }
            else
            {
                _logger.LogError($"[MOCK] Teslimat bildirimi başarısız oldu: Sipariş ID: {notification.OrderId} - Simüle Edilmiş Hata.");
                return false;
            }
        }
    }
}
