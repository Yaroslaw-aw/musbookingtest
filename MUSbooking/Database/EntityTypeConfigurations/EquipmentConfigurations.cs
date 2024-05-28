using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MUSbooking.Database.Models;
using MUSbooking.Database.Models.Connections;

namespace MUSbooking.Database.EntityTypeConfigurations
{
    public class EquipmentConfigurations : IEntityTypeConfiguration<Equipment>
    {
        public void Configure(EntityTypeBuilder<Equipment> builder)
        {
            builder.ToTable("equipments");

            builder.HasIndex(equipment => equipment.Name)
                .IsUnique();

            builder.Property(equipment => equipment.Amount)
                .IsRequired();

            builder.Property(equipment => equipment.Price)
                .IsRequired();

            builder
                .HasMany(equipment => equipment.Orders)
                .WithMany(order => order.Equipments)
                .UsingEntity<OrderEquipments>(
                    order => order
                            .HasOne(orderEquipment => orderEquipment.Order)
                            .WithMany(order => order.OrderEquipments)
                            .HasForeignKey(orderEquipment => orderEquipment.OrderId),
                    equipment => equipment
                            .HasOne(orderEquipment => orderEquipment.Equipment)
                            .WithMany(equipment => equipment.OrderEquipments)
                            .HasForeignKey(orderEquipment => orderEquipment.EquipmentId));
        }
    }
}
