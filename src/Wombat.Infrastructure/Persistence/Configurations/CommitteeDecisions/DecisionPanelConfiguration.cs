using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Infrastructure.Persistence.Configurations.CommitteeDecisions;

public sealed class DecisionPanelConfiguration : IEntityTypeConfiguration<DecisionPanel>
{
    public void Configure(EntityTypeBuilder<DecisionPanel> builder)
    {
        builder.ToTable("DecisionPanels");
        builder.Property(entity => entity.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(entity => entity.Name);

        builder.HasMany(entity => entity.Members)
            .WithOne(entity => entity.Panel)
            .HasForeignKey(entity => entity.PanelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(entity => entity.Reviews)
            .WithOne(entity => entity.Panel)
            .HasForeignKey(entity => entity.PanelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
