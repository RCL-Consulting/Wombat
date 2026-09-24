using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Institutions;

namespace Wombat.Infrastructure.Persistence.Configurations.CommitteeDecisions;

public sealed class DecisionPanelConfiguration : IEntityTypeConfiguration<DecisionPanel>
{
    public void Configure(EntityTypeBuilder<DecisionPanel> builder)
    {
        builder.ToTable("DecisionPanels");
        builder.Property(entity => entity.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(entity => entity.Name);

        // Every panel sits at one institution (T182): the committee rules compare it with the trainee's, and a panel
        // without one matched nobody, so it could review no one.
        builder.HasOne<Institution>()
            .WithMany()
            .HasForeignKey(entity => entity.InstitutionId)
            .OnDelete(DeleteBehavior.Restrict);

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
