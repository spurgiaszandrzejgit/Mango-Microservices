using Mango.Services.ShoppingCartAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.ShoppingCartAPI.DbContexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<CartHeader> CartHeaders => Set<CartHeader>();
        public DbSet<CartDetails> CartDetails => Set<CartDetails>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1 header per user
            modelBuilder.Entity<CartHeader>()
                .HasIndex(x => x.UserId)
                .IsUnique();

            // 1 item per product header
            modelBuilder.Entity<CartDetails>()
                .HasIndex(x => new { x.CartHeaderId, x.ProductId })
                .IsUnique();

            modelBuilder.Entity<CartHeader>()
                .HasMany<CartDetails>()
                .WithOne(d => d.CartHeader)
                .HasForeignKey(d => d.CartHeaderId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }

}
