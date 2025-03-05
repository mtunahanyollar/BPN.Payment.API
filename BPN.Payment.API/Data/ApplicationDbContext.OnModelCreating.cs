using BPN.Payment.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BPN.Payment.API.Data
{
    public partial class ApplicationDbContext
    {
        //Can be partially handled for Product aaaaand Order
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>()
                .HasKey(o => o.Id);

            modelBuilder.Entity<OrderItem>()
                .HasKey(oi => oi.Id);

            modelBuilder.Entity<Product>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Name)
                .HasDatabaseName("idx_product_name");

            modelBuilder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}
