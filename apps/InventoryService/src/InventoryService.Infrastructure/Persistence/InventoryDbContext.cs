using InventoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Persistence;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) { }

    public DbSet<Resource> Resources => Set<Resource>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Resource>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).IsRequired().HasMaxLength(200);
            entity.Property(r => r.Description).IsRequired().HasMaxLength(1000);
            entity.Property(r => r.Location).IsRequired().HasMaxLength(500);
            entity.Property(r => r.PricePerUnit).HasColumnType("decimal(18,2)");
            entity.Property(r => r.Currency).HasMaxLength(3);
            entity.Property(r => r.Type).HasConversion<string>();
        });
    }
}
