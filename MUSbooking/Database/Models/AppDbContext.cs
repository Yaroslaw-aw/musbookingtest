using Microsoft.EntityFrameworkCore;
using MUSbooking.Database.EntityTypeConfigurations;
using MUSbooking.Database.Models.Connections;

namespace MUSbooking.Database.Models
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderEquipments> OrderEquipments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new EquipmentConfigurations());

            modelBuilder.ApplyConfiguration(new OrderConfigurations());
        }
    }
}
