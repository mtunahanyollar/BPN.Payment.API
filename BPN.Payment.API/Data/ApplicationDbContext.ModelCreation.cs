namespace BPN.Payment.API.Data
{
    public partial class ApplicationDbContext
    {
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Order>()
				.HasKey(o => o.Id);

			modelBuilder.Entity<OrderItem>()
				.HasKey(oi => oi.Id);

			modelBuilder.Entity<Product>()
				.HasKey(p => p.Id);

			modelBuilder.Entity<Order>()
				.HasMany(o => o.Items)
				.WithOne()
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
