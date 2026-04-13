using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Infrastructure.Persistence.Configurations.MultiSourceFeedback;

public sealed class MsfCampaignConfiguration : IEntityTypeConfiguration<MsfCampaign>
{
    public void Configure(EntityTypeBuilder<MsfCampaign> builder)
    {
        builder.ToTable("MsfCampaigns");
        builder.Property(entity => entity.SubjectUserId).HasMaxLength(450).IsRequired();
        builder.Property(entity => entity.CreatedByUserId).HasMaxLength(450).IsRequired();
        builder.Property(entity => entity.OpensOn).HasColumnType("date");
        builder.Property(entity => entity.ClosesOn).HasColumnType("date");
        builder.Property(entity => entity.CoordinatorNarrative).HasMaxLength(4000);
        builder.Property(entity => entity.ReviewedByUserId).HasMaxLength(450);

        builder.HasIndex(entity => new { entity.SubjectUserId, entity.State });
        builder.HasIndex(entity => new { entity.TemplateId, entity.ClosesOn });

        builder.HasOne(entity => entity.Template)
            .WithMany()
            .HasForeignKey(entity => entity.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
