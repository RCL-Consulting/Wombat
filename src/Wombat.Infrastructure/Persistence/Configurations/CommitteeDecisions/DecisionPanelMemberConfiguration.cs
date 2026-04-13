using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Infrastructure.Persistence.Configurations.CommitteeDecisions;

public sealed class DecisionPanelMemberConfiguration : IEntityTypeConfiguration<DecisionPanelMember>
{
    public void Configure(EntityTypeBuilder<DecisionPanelMember> builder)
    {
        builder.ToTable("DecisionPanelMembers");
        builder.Property(entity => entity.UserId).HasMaxLength(450).IsRequired();
        builder.HasIndex(entity => new { entity.PanelId, entity.UserId }).IsUnique();
    }
}
