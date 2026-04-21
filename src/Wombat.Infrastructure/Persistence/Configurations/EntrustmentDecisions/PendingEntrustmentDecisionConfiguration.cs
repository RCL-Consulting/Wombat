using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Infrastructure.Persistence.Configurations.EntrustmentDecisions;

public sealed class PendingEntrustmentDecisionConfiguration : IEntityTypeConfiguration<PendingEntrustmentDecision>
{
    public void Configure(EntityTypeBuilder<PendingEntrustmentDecision> builder)
    {
        builder.ToTable("PendingEntrustmentDecisions");

        builder.Property(entity => entity.Rationale).HasMaxLength(4000).IsRequired();
        builder.Property(entity => entity.EvidenceLinksJson).HasColumnType("jsonb").IsRequired();
        builder.Property(entity => entity.StagedByUserId).HasMaxLength(450).IsRequired();

        builder.Property(entity => entity.IssuedOn).HasColumnType("date");
        builder.Property(entity => entity.ExpiresOn).HasColumnType("date");

        builder.HasOne(entity => entity.Review)
            .WithMany()
            .HasForeignKey(entity => entity.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(entity => entity.Epa)
            .WithMany()
            .HasForeignKey(entity => entity.EpaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.AuthorisedLevel)
            .WithMany()
            .HasForeignKey(entity => entity.AuthorisedLevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(entity => new { entity.ReviewId, entity.EpaId });
    }
}
