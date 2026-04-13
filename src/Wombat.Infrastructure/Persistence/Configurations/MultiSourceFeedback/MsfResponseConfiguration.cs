using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Infrastructure.Persistence.Configurations.MultiSourceFeedback;

public sealed class MsfResponseConfiguration : IEntityTypeConfiguration<MsfResponse>
{
    public void Configure(EntityTypeBuilder<MsfResponse> builder)
    {
        builder.ToTable("MsfResponses");
        builder.HasIndex(entity => entity.InvitationId).IsUnique();
        builder.HasIndex(entity => entity.CampaignId);

        builder.HasOne(entity => entity.Campaign)
            .WithMany(campaign => campaign.Responses)
            .HasForeignKey(entity => entity.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(entity => entity.Invitation)
            .WithMany(invitation => invitation.Responses)
            .HasForeignKey(entity => entity.InvitationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
