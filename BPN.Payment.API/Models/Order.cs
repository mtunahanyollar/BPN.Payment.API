using System.ComponentModel.DataAnnotations;

namespace BPN.Payment.API.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();

        public decimal TotalPrice => Items.Sum(i => i.Quantity * i.Price);
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
