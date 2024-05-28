using MUSbooking.Database.Models.Connections;

namespace MUSbooking.Database.Models
{
    public class Equipment
    {
        public Guid EquipmentId { get; set; }
        public string? Name { get; set; }
        public int Amount { get; set; }
        public decimal Price { get; set; }

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

        [System.Text.Json.Serialization.JsonIgnore]
        public List<OrderEquipments> OrderEquipments { get; set; } = new List<OrderEquipments>();
    }
}
