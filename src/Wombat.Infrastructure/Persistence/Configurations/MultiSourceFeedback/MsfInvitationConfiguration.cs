using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Application.Common.Security;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Infrastructure.Persistence.Configurations.MultiSourceFeedback;

public sealed class MsfInvitationConfiguration : IEntityTypeConfiguration<MsfInvitation>
{
    public void Configure(EntityTypeBuilder<MsfInvitation> builder)
    {
        builder.ToTable("MsfInvitations");
        builder.Property(entity => entity.RespondentEmail).HasMaxLength(320);
        builder.Property(entity => entity.TeachingContext).HasMaxLength(MsfTeachingContexts.MaximumLength);
        builder.Property(entity => entity.TokenSelector).HasMaxLength(InvitationTokenService.SelectorLength);
        builder.Property(entity => entity.TokenHash).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.ExpiresOn).HasColumnType("date");

        // The respondent page's one read: a link names its row by the selector its token begins with (T163). Unique, so
        // a selector finds one row or none; PostgreSQL does not count NULLs as equal, so every invitee not yet sent a
        // link can hold none. The hash is not indexed: nothing looks a row up by it, only checks it once the row is found.
        builder.HasIndex(entity => entity.TokenSelector).IsUnique();
        builder.HasIndex(entity => new { entity.CampaignId, entity.RespondentCategory });

        builder.HasOne(entity => entity.Campaign)
            .WithMany(campaign => campaign.Invitations)
            .HasForeignKey(entity => entity.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
