using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Infrastructure.Persistence.Configurations.EntrustmentDecisions;

public sealed class EntrustmentEvidenceLinkConfiguration : IEntityTypeConfiguration<EntrustmentEvidenceLink>
{
    public void Configure(EntityTypeBuilder<EntrustmentEvidenceLink> builder)
    {
        builder.ToTable("EntrustmentEvidenceLinks");

        builder.Property(entity => entity.SourceType).HasConversion<int>();
        builder.Property(entity => entity.SourceLabel).HasMaxLength(200).IsRequired();
        // As wide as the snapshot line it is copied from (CommitteeEvidenceItems.Summary), so the copy is whole (T131).
        builder.Property(entity => entity.Summary).HasMaxLength(4000).IsRequired();

        // T131: the snapshot row the link was built from. No foreign key, for the snapshot's own reason (T167): a STAR and
        // the review that froze its evidence must each outlive, and never block, a change to the other.
        builder.Property(entity => entity.CommitteeEvidenceId);

        builder.HasIndex(entity => new { entity.DecisionId, entity.SourceType });
    }
}
