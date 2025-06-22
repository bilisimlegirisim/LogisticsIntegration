using LogisticsIntegration.Domain.Entities;

namespace LogisticsIntegration.Web.Models
{
    public class OrderListViewModel
    {
        public IEnumerable<Order> Orders { get; set; }
    }
}
