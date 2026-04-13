using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Infrastructure.Persistence.Configurations.CommitteeDecisions;

public sealed class CommitteeAppealConfiguration : IEntityTypeConfiguration<CommitteeAppeal>
{
    public void Configure(EntityTypeBuilder<CommitteeAppeal> builder)
    {
        builder.ToTable("CommitteeAppeals");
        builder.Property(entity => entity.LodgedByUserId).HasMaxLength(450).IsRequired();
        builder.Property(entity => entity.ResolvedByUserId).HasMaxLength(450);
        builder.Property(entity => entity.Reason).HasMaxLength(4000).IsRequired();
        builder.HasIndex(entity => new { entity.ReviewId, entity.LodgedOn });
    }
}
