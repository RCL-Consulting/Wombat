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

        // Optimistic concurrency on the aggregate, and it is load-bearing rather than defensive since
        // T121. The once-only guarantee on the evidence fan-out rests on MsfCampaign.Release refusing
        // anything but UnderReview, and that refusal is an in-memory check on a row both of two
        // simultaneous releases would have read as UnderReview. Without a token both would pass, both
        // would save, and the trainee would get two full sets of permanent evidence activities.
        // Postgres's own xmin costs no column and no write path.
        builder.Property<uint>("xmin").HasColumnName("xmin").HasColumnType("xid").ValueGeneratedOnAddOrUpdate().IsConcurrencyToken();

        builder.HasIndex(entity => new { entity.SubjectUserId, entity.State });
        builder.HasIndex(entity => new { entity.TemplateId, entity.ClosesOn });

        builder.HasOne(entity => entity.Template)
            .WithMany()
            .HasForeignKey(entity => entity.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
