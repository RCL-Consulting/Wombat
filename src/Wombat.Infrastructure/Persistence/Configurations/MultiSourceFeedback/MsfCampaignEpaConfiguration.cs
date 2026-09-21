using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Infrastructure.Persistence.Configurations.MultiSourceFeedback;

public sealed class MsfCampaignEpaConfiguration : IEntityTypeConfiguration<MsfCampaignEpa>
{
    public void Configure(EntityTypeBuilder<MsfCampaignEpa> builder)
    {
        builder.ToTable("MsfCampaignEpas");

        // The pair is the row's meaning: an EPA is covered or it is not. Without the unique index a
        // double-submit on the creation form would make the release fan out two identical evidence
        // activities for the same EPA.
        builder.HasIndex(entity => new { entity.CampaignId, entity.EpaId }).IsUnique();

        builder.HasOne(entity => entity.Campaign)
            .WithMany(campaign => campaign.CoveredEpas)
            .HasForeignKey(entity => entity.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict, like every other reference to the national catalogue: an EPA that some campaign
        // declared itself evidence for must not be deletable out from under the record.
        builder.HasOne(entity => entity.Epa)
            .WithMany()
            .HasForeignKey(entity => entity.EpaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
