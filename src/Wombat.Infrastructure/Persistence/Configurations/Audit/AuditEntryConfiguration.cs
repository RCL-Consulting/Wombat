using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Audit;

namespace Wombat.Infrastructure.Persistence.Configurations.Audit;

public sealed class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("AuditEntries");

        builder.HasKey(e => e.Id);

        // Guid v7 — time-sortable; generated in application code, not by DB.
        builder.Property(e => e.Id)
            .HasColumnType("uuid")
            .ValueGeneratedNever();

        builder.Property(e => e.OccurredAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(e => e.ActorUserId)
            .HasMaxLength(450);

        builder.Property(e => e.ActorDisplay)
            .HasMaxLength(200);

        builder.Property(e => e.ActorIpAddress)
            .HasMaxLength(50);

        builder.Property(e => e.ActorUserAgent)
            .HasMaxLength(500);

        builder.Property(e => e.Category)
            .HasColumnType("integer")
            .IsRequired();

        builder.Property(e => e.Action)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.SubjectType)
            .HasMaxLength(100);

        builder.Property(e => e.SubjectId)
            .HasColumnType("uuid");

        builder.Property(e => e.InstitutionId)
            .HasColumnType("integer");

        builder.Property(e => e.SpecialityId)
            .HasColumnType("integer");

        builder.Property(e => e.SummaryJson)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(e => e.Success)
            .HasColumnType("boolean")
            .IsRequired();

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(2000);

        // Time-range queries (most common access pattern)
        builder.HasIndex(e => e.OccurredAt);

        // "What did user X do?"
        builder.HasIndex(e => new { e.ActorUserId, e.OccurredAt });

        // "What happened to object Y?"
        builder.HasIndex(e => new { e.SubjectType, e.SubjectId, e.OccurredAt });

        // Ad-hoc field searches on SummaryJson (rare but useful for admins)
        builder.HasIndex(e => e.SummaryJson)
            .HasMethod("gin");
    }
}
