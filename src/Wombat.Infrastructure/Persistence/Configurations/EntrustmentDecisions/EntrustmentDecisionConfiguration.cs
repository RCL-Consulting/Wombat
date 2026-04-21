using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Infrastructure.Persistence.Configurations.EntrustmentDecisions;

public sealed class EntrustmentDecisionConfiguration : IEntityTypeConfiguration<EntrustmentDecision>
{
    public void Configure(EntityTypeBuilder<EntrustmentDecision> builder)
    {
        builder.ToTable("EntrustmentDecisions");

        builder.Property(entity => entity.TraineeUserId).HasMaxLength(450).IsRequired();
        builder.Property(entity => entity.IssuedByChairUserId).HasMaxLength(450).IsRequired();
        builder.Property(entity => entity.RevokedByUserId).HasMaxLength(450);
        builder.Property(entity => entity.Rationale).HasMaxLength(4000).IsRequired();
        builder.Property(entity => entity.RevocationReason).HasMaxLength(1000);

        builder.Property(entity => entity.IssuedOn).HasColumnType("date");
        builder.Property(entity => entity.ExpiresOn).HasColumnType("date");
        builder.Property(entity => entity.LastExpiryReminderSentOn).HasColumnType("date");

        builder.Property(entity => entity.Status).HasConversion<int>();

        builder.HasOne(entity => entity.Epa)
            .WithMany()
            .HasForeignKey(entity => entity.EpaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.AuthorisedLevel)
            .WithMany()
            .HasForeignKey(entity => entity.AuthorisedLevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.IssuedByCommitteeReview)
            .WithMany()
            .HasForeignKey(entity => entity.IssuedByCommitteeReviewId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(entity => entity.EvidenceLinks)
            .WithOne(entity => entity.Decision)
            .HasForeignKey(entity => entity.DecisionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(EntrustmentDecision.EvidenceLinks))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(entity => new { entity.TraineeUserId, entity.EpaId, entity.Status });
        builder.HasIndex(entity => new { entity.Status, entity.ExpiresOn });
        builder.HasIndex(entity => entity.IssuedByCommitteeReviewId);
    }
}
