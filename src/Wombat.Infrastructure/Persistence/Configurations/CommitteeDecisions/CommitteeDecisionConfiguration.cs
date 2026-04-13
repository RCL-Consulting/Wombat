using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Infrastructure.Persistence.Configurations.CommitteeDecisions;

public sealed class CommitteeDecisionConfiguration : IEntityTypeConfiguration<CommitteeDecision>
{
    public void Configure(EntityTypeBuilder<CommitteeDecision> builder)
    {
        builder.ToTable("CommitteeDecisions");
        builder.Property(entity => entity.Rationale).HasMaxLength(4000).IsRequired();
        builder.Property(entity => entity.Conditions).HasMaxLength(4000);
        builder.Property(entity => entity.DecidedByChairUserId).HasMaxLength(450).IsRequired();
        builder.HasIndex(entity => new { entity.ReviewId, entity.DecidedOn });
    }
}
