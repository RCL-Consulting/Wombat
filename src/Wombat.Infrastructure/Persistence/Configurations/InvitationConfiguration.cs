using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Invitations;

namespace Wombat.Infrastructure.Persistence.Configurations;

public sealed class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.ToTable("Invitations", table =>
        {
            // A link's mail was sent or dropped, never both (T283). The worker's report writes both times in one
            // statement, one of them null, and a resend clears the drop with the link it replaces.
            table.HasCheckConstraint(
                "CK_Invitations_DeliveryOutcome",
                "\"SentOn\" IS NULL OR \"DeliveryFailedOn\" IS NULL");
            table.HasCheckConstraint(
                "CK_Invitations_DeliveryFailures",
                "\"DeliveryFailures\" >= 0");
        });
        builder.Property(entity => entity.Email).HasMaxLength(320).IsRequired();
        builder.Property(entity => entity.TokenHash).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.TargetRole).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.IssuedByUserId).HasMaxLength(450).IsRequired();
        builder.Property(entity => entity.ExpiresOn).HasColumnType("date");

        builder.HasIndex(entity => entity.TokenHash);
        builder.HasIndex(entity => new { entity.Email, entity.ExpiresOn });
    }
}
