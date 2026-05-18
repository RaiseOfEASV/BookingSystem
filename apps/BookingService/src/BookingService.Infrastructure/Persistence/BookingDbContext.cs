using BookingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure.Persistence;

public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options) { }

    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingItem> BookingItems => Set<BookingItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.TotalPrice).HasColumnType("decimal(18,2)");
            entity.Property(b => b.Status).HasConversion<string>();
            entity.Property(b => b.Notes).HasMaxLength(1000);

            entity.HasIndex(b => b.CustomerId);

            entity.HasMany(b => b.Items)
                  .WithOne(i => i.Booking)
                  .HasForeignKey(i => i.BookingId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BookingItem>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.ResourceType).HasConversion<string>().HasMaxLength(50);
            entity.Property(i => i.ResourceLocation).IsRequired().HasMaxLength(500);
            entity.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(i => i.SubTotal).HasColumnType("decimal(18,2)");

            entity.HasIndex(i => i.ResourceId);
            entity.HasIndex(i => new { i.ResourceId, i.StartTime, i.EndTime });
        });
    }
}
