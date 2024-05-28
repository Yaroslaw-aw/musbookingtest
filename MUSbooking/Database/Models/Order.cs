using MUSbooking.Database.Models.Connections;

namespace MUSbooking.Database.Models
{
    public class Order
    {
        public Guid OrderId { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public decimal Price { get; set; } = default;

        public virtual ICollection<Equipment> Equipments { get; set; } = new List<Equipment>();

        [System.Text.Json.Serialization.JsonIgnore]
        public List<OrderEquipments> OrderEquipments { get; set; } = new List<OrderEquipments>();
    }
}
