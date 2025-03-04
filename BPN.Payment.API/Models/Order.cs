using System.ComponentModel.DataAnnotations;

namespace BPN.Payment.API.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();

        public decimal TotalPrice => Items.Sum(i => i.Quantity * i.Price);
    }

    public class OrderItem
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int ProductId { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        public decimal Price { get; set; }
    }
}
