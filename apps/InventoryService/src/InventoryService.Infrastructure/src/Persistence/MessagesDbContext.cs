using InventoryService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Persistence;

public class MessagesDbContext : DbContext
{
    public MessagesDbContext(DbContextOptions<MessagesDbContext> options) : base(options) { }

    public DbSet<MessageEntity> Messages => Set<MessageEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MessageEntity>(entity =>
        {
            entity.ToTable("messages");
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(m => m.EventId).HasColumnName("event_id");
            entity.Property(m => m.SeatId).HasColumnName("seat_id");
            entity.Property(m => m.CorrelationId).HasColumnName("correlation_id");
            entity.Property(m => m.Status).HasColumnName("status").IsRequired().HasMaxLength(50);
            entity.Property(m => m.RetryCount).HasColumnName("retry_count").HasDefaultValue(0);
            entity.Property(m => m.LastError).HasColumnName("last_error").HasMaxLength(2000);
            entity.Property(m => m.ReceivedAt).HasColumnName("received_at");
            entity.Property(m => m.UpdatedAt).HasColumnName("updated_at");
            entity.Property(m => m.ProcessedAt).HasColumnName("processed_at");
            
            entity.HasIndex(m => new { m.Status, m.UpdatedAt })
                .HasDatabaseName("IX_messages_status_updated_at");
        });
    }
}