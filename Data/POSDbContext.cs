using ClientAppPOSWebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ClientAppPOSWebAPI.Data
{
    public class POSDbContext : DbContext
    {
        public POSDbContext(DbContextOptions<POSDbContext> options) : base(options)
        { }

        public DbSet<Product> Products { get; set; }
        public DbSet<ThemeSettings> ThemeSettings { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }



    }
}
