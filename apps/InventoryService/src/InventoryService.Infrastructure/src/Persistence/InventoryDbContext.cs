using InventoryService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Persistence;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) { }

    public DbSet<Venue>         Venues          => Set<Venue>();
    public DbSet<Seat>          Seats           => Set<Seat>();
    public DbSet<Event>         Events          => Set<Event>();
    public DbSet<SeatInventory> SeatInventories => Set<SeatInventory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Venue>(entity =>
        {
            entity.ToTable("venues");
            entity.HasKey(v => v.Id);
            entity.Property(v => v.Id).HasColumnName("id").HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(v => v.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
            entity.Property(v => v.Type).HasColumnName("type").IsRequired().HasMaxLength(100);
            entity.Property(v => v.Address).HasColumnName("address").IsRequired().HasMaxLength(500);
        });

        modelBuilder.Entity<Seat>(entity =>
        {
            entity.ToTable("seats");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Id).HasColumnName("id").HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(s => s.VenueId).HasColumnName("venue_id");
            entity.Property(s => s.Row).HasColumnName("row").IsRequired().HasMaxLength(10);
            entity.Property(s => s.Number).HasColumnName("number");
            entity.Property(s => s.Section).HasColumnName("section").IsRequired().HasMaxLength(100);

            entity.HasIndex(s => new { s.VenueId, s.Row, s.Number, s.Section }).IsUnique();

            entity.HasOne(s => s.Venue)
                  .WithMany(v => v.Seats)
                  .HasForeignKey(s => s.VenueId);
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("events");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.VenueId).HasColumnName("venue_id");
            entity.Property(e => e.EventName).HasColumnName("event_name").IsRequired().HasMaxLength(300);
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.EndDate).HasColumnName("end_date");

            entity.HasOne(e => e.Venue)
                  .WithMany(v => v.Events)
                  .HasForeignKey(e => e.VenueId);
        });

        modelBuilder.Entity<SeatInventory>(entity =>
        {
            entity.ToTable("seats_inventory");
            entity.HasKey(si => si.Id);
            entity.Property(si => si.Id).HasColumnName("id").HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(si => si.EventId).HasColumnName("event_id");
            entity.Property(si => si.SeatId).HasColumnName("seat_id");
            entity.Property(si => si.Status).HasColumnName("status").IsRequired().HasMaxLength(50);
            entity.Property(si => si.Version).HasColumnName("version").IsConcurrencyToken();
            entity.Property(si => si.HeldBy).HasColumnName("held_by");
            entity.Property(si => si.HeldAt).HasColumnName("held_at");

            entity.HasIndex(si => new { si.EventId, si.SeatId }).IsUnique();

            entity.HasOne(si => si.Event)
                  .WithMany(e => e.SeatInventories)
                  .HasForeignKey(si => si.EventId);

            entity.HasOne(si => si.Seat)
                  .WithMany(s => s.SeatInventories)
                  .HasForeignKey(si => si.SeatId);
        });
    }
}
