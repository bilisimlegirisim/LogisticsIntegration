using LogisticsIntegration.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogisticsIntegration.ApiClients.BCompany.Interfaces
{
    public interface ICustomerDeliveryApiClient
    {
        Task<bool> NotifyCustomerDeliveryAsync(CustomerDeliveryNotificationDTO notification);
    }
}
