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
        builder.Property(entity => entity.Summary).HasMaxLength(2000).IsRequired();

        builder.HasIndex(entity => new { entity.DecisionId, entity.SourceType });
    }
}
