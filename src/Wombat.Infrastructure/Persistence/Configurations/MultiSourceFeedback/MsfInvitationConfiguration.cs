using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Application.Common.Security;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Infrastructure.Persistence.Configurations.MultiSourceFeedback;

public sealed class MsfInvitationConfiguration : IEntityTypeConfiguration<MsfInvitation>
{
    /// <summary>The address as invitations are compared, lower-cased by the database; a shadow property. (T228)</summary>
    public const string RespondentEmailKey = "RespondentEmailKey";

    /// <summary>One invitation per campaign and address while the address is held. (T228)</summary>
    public const string RespondentEmailKeyIndex = "IX_MsfInvitations_CampaignId_RespondentEmailKey";

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

            // A link's mail was sent or dropped, never both (T251). The worker's report writes the selector and both
            // times in one statement, one of them null; anonymising nulls all three (MsfInvitation.Anonymize), and every
            // save of an anonymised invitation writes them, whatever it read (ApplicationDbContext, T251 review). Without
            // that, a close that read an invitation before a report landed would write only the columns it saw change
            // and leave the report's time on the erased row, one mail apart from the next, beside a log line naming the
            // address it was sent to at that instant.
            table.HasCheckConstraint(
                "CK_MsfInvitations_DeliveryOutcome",
                "\"SentOn\" IS NULL OR \"DeliveryFailedOn\" IS NULL");
        });
        builder.Property(entity => entity.RespondentEmail).HasMaxLength(320);
        builder.Property(entity => entity.TeachingContext).HasMaxLength(MsfTeachingContexts.MaximumLength);
        builder.Property(entity => entity.TokenSelector).HasMaxLength(InvitationTokenService.SelectorLength);
        builder.Property(entity => entity.TokenHash).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.PreviousTokenSelector).HasMaxLength(InvitationTokenService.SelectorLength);
        builder.Property(entity => entity.PreviousTokenHash).HasMaxLength(64);
        builder.Property(entity => entity.DeliveryLinkSelector).HasMaxLength(InvitationTokenService.SelectorLength);
        builder.Property(entity => entity.ExpiresOn).HasColumnType("date");

        // The respondent page's one read: a link names its row by the selector its token begins with (T163). Unique, so
        // a selector finds one row or none; PostgreSQL does not count NULLs as equal, so every invitee not yet sent a
        // link can hold none. The hash is not indexed: nothing looks a row up by it, only checks it once the row is found.
        builder.HasIndex(entity => entity.TokenSelector).IsUnique();

        // The link a reminder replaced, which takes a response until the last day to respond, is found the same way, by
        // a unique index of its own (T214). The lookup asks both columns in one statement.
        builder.HasIndex(entity => entity.PreviousTokenSelector).IsUnique();

        builder.HasIndex(entity => new { entity.CampaignId, entity.RespondentCategory });

        // A campaign invites an address once, in any capitals (T228). Until T228 an address added twice was mailed two
        // working links, so one respondent could answer twice and fill a group's minimum alone. The add command refuses
        // it first (MsfInvitation.AddressKey); this holds it for every writer and for two adds that race.
        //
        // The key is a stored generated column, so the rule is in the model rather than in a migration's raw SQL, and
        // it follows the address: anonymising an invitation nulls the address (MsfInvitation.Anonymize, T207), and
        // PostgreSQL recomputes the key to NULL in the same update, so nothing derived from the address outlives it.
        // While the address is held, the key tells no one more than the address beside it. An erased address leaves
        // the index (the filter), and so does not block anything. Stored, because PostgreSQL 18 makes a generated
        // column virtual by default, and a virtual one cannot be indexed.
        builder.Property<string?>(RespondentEmailKey)
            .HasMaxLength(320)
            .HasComputedColumnSql("lower(\"RespondentEmail\")", stored: true);
        builder.HasIndex(nameof(MsfInvitation.CampaignId), RespondentEmailKey)
            .IsUnique()
            .HasFilter($"\"{RespondentEmailKey}\" IS NOT NULL")
            .HasDatabaseName(RespondentEmailKeyIndex);

        builder.HasOne(entity => entity.Campaign)
            .WithMany(campaign => campaign.Invitations)
            .HasForeignKey(entity => entity.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
