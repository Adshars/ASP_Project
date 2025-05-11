using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using WarehouseAPI.Model;

namespace WarehouseAPI.Data
{
    public class WarehouseDbContext : IdentityDbContext<User>
    {
        public WarehouseDbContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
    }
}
