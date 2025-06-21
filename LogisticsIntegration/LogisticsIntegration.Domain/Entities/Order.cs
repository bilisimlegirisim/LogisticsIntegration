using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogisticsIntegration.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string ShippingPoint { get; set; }
        public string RecipientName { get; set; }
        public string ContactPhone { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public string CustomerOrderId { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
    }

    public enum OrderStatus
    {
        Pending = 0,
        Delivered = 1,
        Completed = 2
    }
}
