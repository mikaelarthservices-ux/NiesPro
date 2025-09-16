using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Infrastructure.EventStore;

namespace Order.Infrastructure.Configurations;

public sealed class StoredEventConfiguration : IEntityTypeConfiguration<StoredEvent>
{
    public void Configure(EntityTypeBuilder<StoredEvent> builder)
    {
        builder.ToTable("EventStore");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.AggregateId)
            .IsRequired();

        builder.Property(e => e.AggregateType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.EventType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.EventData)
            .IsRequired()
            .HasColumnType("LONGTEXT");

        builder.Property(e => e.Metadata)
            .HasColumnType("LONGTEXT");

        builder.Property(e => e.Version)
            .IsRequired();

        builder.Property(e => e.Timestamp)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

        builder.Property(e => e.CorrelationId)
            .HasMaxLength(100);

        builder.Property(e => e.CausationId)
            .HasMaxLength(100);

        // Index uniques et de performance
        builder.HasIndex(e => new { e.AggregateId, e.Version })
            .IsUnique()
            .HasDatabaseName("IX_EventStore_Aggregate_Version");

        builder.HasIndex(e => e.AggregateId)
            .HasDatabaseName("IX_EventStore_AggregateId");

        builder.HasIndex(e => e.EventType)
            .HasDatabaseName("IX_EventStore_EventType");

        builder.HasIndex(e => e.Timestamp)
            .HasDatabaseName("IX_EventStore_Timestamp");

        builder.HasIndex(e => new { e.AggregateType, e.Timestamp })
            .HasDatabaseName("IX_EventStore_AggregateType_Timestamp");

        // Contraintes
        builder.ToTable(t => 
        {
            t.HasCheckConstraint("CK_EventStore_Version", "Version > 0");
        });
    }
}