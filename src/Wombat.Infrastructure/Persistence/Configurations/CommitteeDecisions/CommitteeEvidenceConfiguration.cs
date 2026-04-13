using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Infrastructure.Persistence.Configurations.CommitteeDecisions;

public sealed class CommitteeEvidenceConfiguration : IEntityTypeConfiguration<CommitteeEvidence>
{
    public void Configure(EntityTypeBuilder<CommitteeEvidence> builder)
    {
        builder.ToTable("CommitteeEvidenceItems");
        builder.Property(entity => entity.SourceLabel).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.Summary).HasMaxLength(4000).IsRequired();
        builder.HasIndex(entity => new { entity.ReviewId, entity.SourceType });
    }
}
