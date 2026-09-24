using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Application.Common.Security;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Infrastructure.Persistence.Configurations.MultiSourceFeedback;

public sealed class MsfInvitationConfiguration : IEntityTypeConfiguration<MsfInvitation>
{
    public void Configure(EntityTypeBuilder<MsfInvitation> builder)
    {
        builder.ToTable("MsfInvitations", table =>
        {
            // A link a reminder replaced is stored as a selector and a hash, set and retired together (T214): a selector
            // alone would find a row no token can open, and a hash alone would be a credential no link can reach.
            table.HasCheckConstraint(
                "CK_MsfInvitations_PreviousLink",
                "(\"PreviousTokenSelector\" IS NULL) = (\"PreviousTokenHash\" IS NULL)");

            // And never on an answered invitation, whichever write comes last (T214 review). The reminder job reads a
            // respondent, mails them and only then stores the link it replaced; an answer committed in that gap would
            // otherwise have the replaced link put back on its row. The job's store is refused here and it moves on
            // (MsfInvitationExpiryReminderJob). The answer, in the other order, writes that it holds no previous link
            // whatever it read (SubmitMsfResponseCommandHandler), so it always passes.
            table.HasCheckConstraint(
                "CK_MsfInvitations_PreviousLinkUnanswered",
                "\"PreviousTokenSelector\" IS NULL OR \"RespondedOn\" IS NULL");
        });
        builder.Property(entity => entity.RespondentEmail).HasMaxLength(320);
        builder.Property(entity => entity.TeachingContext).HasMaxLength(MsfTeachingContexts.MaximumLength);
        builder.Property(entity => entity.TokenSelector).HasMaxLength(InvitationTokenService.SelectorLength);
        builder.Property(entity => entity.TokenHash).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.PreviousTokenSelector).HasMaxLength(InvitationTokenService.SelectorLength);
        builder.Property(entity => entity.PreviousTokenHash).HasMaxLength(64);
        builder.Property(entity => entity.ExpiresOn).HasColumnType("date");

        // The respondent page's one read: a link names its row by the selector its token begins with (T163). Unique, so
        // a selector finds one row or none; PostgreSQL does not count NULLs as equal, so every invitee not yet sent a
        // link can hold none. The hash is not indexed: nothing looks a row up by it, only checks it once the row is found.
        builder.HasIndex(entity => entity.TokenSelector).IsUnique();

        // The link a reminder replaced, which takes a response until the last day to respond, is found the same way, by
        // a unique index of its own (T214). The lookup asks both columns in one statement.
        builder.HasIndex(entity => entity.PreviousTokenSelector).IsUnique();

        builder.HasIndex(entity => new { entity.CampaignId, entity.RespondentCategory });

        builder.HasOne(entity => entity.Campaign)
            .WithMany(campaign => campaign.Invitations)
            .HasForeignKey(entity => entity.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
