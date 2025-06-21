using LogisticsIntegration.Domain.Entities;
using LogisticsIntegration.Domain.Interfaces;
using LogisticsIntegration.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogisticsIntegration.Infrastructure.Repositories
{
    public class DeliveryRepository : GenericRepository<Delivery>, IDeliveryRepository
    {
        public DeliveryRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
