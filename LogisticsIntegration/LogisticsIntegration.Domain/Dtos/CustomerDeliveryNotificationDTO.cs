using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogisticsIntegration.Domain.Dtos
{
    public class CustomerDeliveryNotificationDTO
    {
        public int OrderId { get; set; }         
        public string CustomerEmail { get; set; } 
        public DateTime DeliveryDate { get; set; }
    }
}
