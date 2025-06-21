using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogisticsIntegration.Domain.Entities
{
    public class Delivery
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string PlateNumber { get; set; }     
        public string DelivererName { get; set; }  
        public bool IsNotifiedToCustomer { get; set; } = false; 

        public DateTime? NotificationDate { get; set; } 

        public Order Order { get; set; } 
    }
}
