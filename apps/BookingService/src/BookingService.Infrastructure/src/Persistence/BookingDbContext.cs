using BookingService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure.Persistence;

public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options) { }

    public DbSet<BookingEntity>     Bookings     => Set<BookingEntity>();
    public DbSet<BookingSagaEntity> BookingSagas => Set<BookingSagaEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BookingEntity>(entity =>
        {
            entity.ToTable("bookings");

            entity.HasKey(b => b.Id);

            entity.Property(b => b.Id)
                  .HasColumnName("id")
                  .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(b => b.CustomerId)
                  .HasColumnName("customer_id")
                  .IsRequired();

            entity.Property(b => b.EventId)
                  .HasColumnName("event_id")
                  .IsRequired();

            entity.Property(b => b.SeatId)
                  .HasColumnName("seat_id")
                  .IsRequired();

            entity.Property(b => b.Amount)
                  .HasColumnName("amount")
                  .HasPrecision(18, 2)
                  .IsRequired();

            entity.Property(b => b.Currency)
                  .HasColumnName("currency")
                  .HasMaxLength(10)
                  .IsRequired();

            entity.Property(b => b.Status)
                  .HasColumnName("status")
                  .HasMaxLength(20)
                  .IsRequired();

            entity.Property(b => b.Code)
                  .HasColumnName("code")
                  .HasMaxLength(30)
                  .IsRequired();

            entity.HasIndex(b => b.Code)
                  .IsUnique()
                  .HasDatabaseName("uq_bookings_code");

            entity.HasIndex(b => new { b.EventId, b.SeatId })
                  .IsUnique()
                  .HasDatabaseName("uq_bookings_event_seat");

            entity.Property(b => b.Notes)
                  .HasColumnName("notes")
                  .HasMaxLength(500);

            entity.Property(b => b.CreatedAt)
                  .HasColumnName("created_at");

            entity.Property(b => b.UpdatedAt)
                  .HasColumnName("updated_at")
                  .IsConcurrencyToken();

            entity.HasIndex(b => new { b.CustomerId, b.Status })
                  .HasDatabaseName("ix_bookings_customer_status");

            entity.HasIndex(b => new { b.EventId, b.SeatId })
                  .HasDatabaseName("ix_bookings_event_seat");

            entity.HasIndex(b => b.Status)
                  .HasDatabaseName("ix_bookings_status");
        });

        modelBuilder.Entity<BookingSagaEntity>(entity =>
        {
            entity.ToTable("booking_sagas");

            entity.HasKey(s => s.Id);

            entity.Property(s => s.Id)
                  .HasColumnName("id")
                  .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(s => s.EventId)
                  .HasColumnName("event_id")
                  .IsRequired();

            entity.Property(s => s.SeatId)
                  .HasColumnName("seat_id")
                  .IsRequired();
              

            entity.Property(s => s.BookingId)
                  .HasColumnName("booking_id");

            entity.Property(s => s.CorrelationId)
                  .HasColumnName("correlation_id")
                  .IsRequired();

            entity.HasIndex(s => s.CorrelationId)
                  .IsUnique()
                  .HasDatabaseName("uq_booking_sagas_correlation_id");

            entity.Property(s => s.PaymentId)
                  .HasColumnName("payment_id")
                  .HasMaxLength(100);

            entity.Property(s => s.Status)
                  .HasColumnName("status")
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(s => s.FailureReason)
                  .HasColumnName("failure_reason")
                  .HasMaxLength(500);

            entity.Property(s => s.CreatedAt)
                  .HasColumnName("created_at");

            entity.Property(s => s.UpdatedAt)
                  .HasColumnName("updated_at")
                  .IsConcurrencyToken();

            // Partial index — only index rows that are still active.
            // Finalized, Failed, and Compensated rows are terminal and never polled.
            entity.HasIndex(s => s.Status)
                  .HasDatabaseName("ix_booking_sagas_active_status")
                  .HasFilter("status NOT IN ('finalized', 'failed', 'compensated')");
        });
    }
}
