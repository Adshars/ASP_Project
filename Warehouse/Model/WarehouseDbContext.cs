using Microsoft.EntityFrameworkCore;

namespace WarehouseAPI.Data
{
    public class WarehouseDbContext : DbContext
    {
        public WarehouseDbContext(DbContextOptions options) : base(options)
        {

        }
    }
}
