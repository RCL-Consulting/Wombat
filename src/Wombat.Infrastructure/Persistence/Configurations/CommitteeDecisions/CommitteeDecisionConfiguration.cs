using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Infrastructure.Persistence.Configurations.CommitteeDecisions;

public sealed class CommitteeDecisionConfiguration : IEntityTypeConfiguration<CommitteeDecision>
{
    public void Configure(EntityTypeBuilder<CommitteeDecision> builder)
    {
        builder.ToTable("CommitteeDecisions");

        // Null on an entrustment-only review's decision, which records no progression category (T131 slice 5). Which a
        // decision carries depends on its review's type, a rule across two tables that CommitteeReview holds.
        builder.Property(entity => entity.Category).IsRequired(false);
        builder.Property(entity => entity.Rationale).HasMaxLength(4000).IsRequired();
        builder.Property(entity => entity.Conditions).HasMaxLength(4000);
        builder.Property(entity => entity.DecidedByChairUserId).HasMaxLength(450).IsRequired();
        builder.HasIndex(entity => new { entity.ReviewId, entity.DecidedOn });

        // T165: who was present when this decision was taken. Each decision keeps its own sitting.
        builder.HasMany(entity => entity.Attendees)
            .WithOne(entity => entity.Decision)
            .HasForeignKey(entity => entity.DecisionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
