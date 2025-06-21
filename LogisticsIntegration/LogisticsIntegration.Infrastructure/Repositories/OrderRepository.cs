using LogisticsIntegration.Domain.Entities;
using LogisticsIntegration.Domain.Interfaces;
using LogisticsIntegration.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogisticsIntegration.Infrastructure.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status)
        {
            return await _dbSet
                .Include(o => o.OrderItems)
                .Where(o => o.Status == status)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersNotNotifiedToCustomer()
        {
            return await _dbContext.Orders
                .Where(o => o.Status == OrderStatus.Delivered &&
                            _dbContext.Deliveries.Any(d => d.OrderId == o.Id && !d.IsNotifiedToCustomer))
                .Include(o => o.OrderItems)
                .ToListAsync();
        }

        // GetByIdAsync metodu, OrderItems'ı da içerecek şekilde güncellendi.
        public new async Task<Order> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);
        }
    }
}
