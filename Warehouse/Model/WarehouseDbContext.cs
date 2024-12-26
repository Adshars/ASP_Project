using Microsoft.EntityFrameworkCore;
using WarehouseAPI.Model;

namespace WarehouseAPI.Data
{
    public class WarehouseDbContext : DbContext
    {
        public WarehouseDbContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
    }
}
