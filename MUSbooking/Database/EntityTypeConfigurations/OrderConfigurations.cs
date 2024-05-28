using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MUSbooking.Database.Models;

namespace MUSbooking.Database.EntityTypeConfigurations
{
    public class OrderConfigurations : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("orders");

            builder.Property(order => order.Description)
                .HasColumnName("description");

            builder.Property(order => order.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(order => order.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(order => order.Price)
                .HasColumnName("price");            
        }
    }
}
