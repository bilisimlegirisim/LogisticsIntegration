using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogisticsIntegration.Domain.Dtos
{
    public class CustomerOrderDto
    {
        public string CustomerOrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string ShippingPoint { get; set; }
        public string RecipientName { get; set; }
        public string ContactPhone { get; set; }
        public List<CustomerOrderItemDto> Items { get; set; }
    }

    public class CustomerOrderItemDto
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }
}
