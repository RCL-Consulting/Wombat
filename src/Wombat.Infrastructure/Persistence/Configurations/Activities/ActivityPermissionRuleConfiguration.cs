using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Activities;

namespace Wombat.Infrastructure.Persistence.Configurations.Activities;

public sealed class ActivityPermissionRuleConfiguration : IEntityTypeConfiguration<ActivityPermissionRule>
{
    public void Configure(EntityTypeBuilder<ActivityPermissionRule> builder)
    {
        builder.ToTable("ActivityPermissionRules");
        builder.Property(entity => entity.TransitionKey).HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.ActorRuleJson).HasColumnType("jsonb").IsRequired();
        builder.Property(entity => entity.FieldRequirementJson).HasColumnType("jsonb");
        builder.HasIndex(entity => new { entity.ActivityTypeId, entity.TransitionKey });
    }
}
