using Microsoft.EntityFrameworkCore;
using PositionsService.Domain.Entities;

namespace PositionsService.Infra.Persistence
{
    public class PositionsDbContext : DbContext
    {
        public DbSet<Position> Positions { get; set; }

        public PositionsDbContext(DbContextOptions<PositionsDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Position>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.InstrumentId).IsRequired().HasMaxLength(20);
                entity.Property(p => p.Quantity).HasPrecision(18, 8);
                entity.Property(p => p.InitialRate).HasPrecision(18, 8);
                entity.Property(p => p.Side).HasConversion<string>();
                entity.Property(p => p.Status).HasConversion<string>();
                entity.HasIndex(p => p.Status);
            });
        }
    }
}