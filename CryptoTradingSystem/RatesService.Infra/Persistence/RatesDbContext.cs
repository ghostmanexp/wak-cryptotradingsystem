using Microsoft.EntityFrameworkCore;
using RatesService.Domain.Entities;

namespace RatesService.Infra.Persistence
{
    public class RatesDbContext : DbContext
    {
        public DbSet<Rate> Rates { get; set; }

        public RatesDbContext(DbContextOptions<RatesDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rate>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Symbol).IsRequired().HasMaxLength(10);
                entity.Property(r => r.Value).HasPrecision(18, 8);
                entity.Property(r => r.Timestamp).IsRequired();
                entity.HasIndex(r => new { r.Symbol, r.Timestamp });
            });
        }
    }
}