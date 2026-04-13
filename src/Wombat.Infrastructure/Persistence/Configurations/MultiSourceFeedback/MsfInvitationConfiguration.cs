using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Infrastructure.Persistence.Configurations.MultiSourceFeedback;

public sealed class MsfInvitationConfiguration : IEntityTypeConfiguration<MsfInvitation>
{
    public void Configure(EntityTypeBuilder<MsfInvitation> builder)
    {
        builder.ToTable("MsfInvitations");
        builder.Property(entity => entity.RespondentEmail).HasMaxLength(320);
        builder.Property(entity => entity.RespondentEmailHash).HasMaxLength(64);
        builder.Property(entity => entity.TokenHash).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.ExpiresOn).HasColumnType("date");

        builder.HasIndex(entity => entity.TokenHash).IsUnique();
        builder.HasIndex(entity => new { entity.CampaignId, entity.RespondentCategory });

        builder.HasOne(entity => entity.Campaign)
            .WithMany(campaign => campaign.Invitations)
            .HasForeignKey(entity => entity.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
