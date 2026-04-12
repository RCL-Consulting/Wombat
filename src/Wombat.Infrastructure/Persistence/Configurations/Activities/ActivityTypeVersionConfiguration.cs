using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Activities;

namespace Wombat.Infrastructure.Persistence.Configurations.Activities;

public sealed class ActivityTypeVersionConfiguration : IEntityTypeConfiguration<ActivityTypeVersion>
{
    public void Configure(EntityTypeBuilder<ActivityTypeVersion> builder)
    {
        builder.ToTable("ActivityTypeVersions");
        builder.Property(entity => entity.SchemaJson).HasColumnType("jsonb").IsRequired();
        builder.Property(entity => entity.WorkflowJson).HasColumnType("jsonb").IsRequired();
        builder.Property(entity => entity.CreditRulesJson).HasColumnType("jsonb").IsRequired();
        builder.Property(entity => entity.DisplayFieldsJson).HasColumnType("jsonb").IsRequired();
        builder.Property(entity => entity.PublishedByUserId).HasMaxLength(450).IsRequired();
        builder.Property(entity => entity.PublishedOn).HasColumnType("timestamp with time zone");

        builder.HasIndex(entity => new { entity.ActivityTypeId, entity.Version }).IsUnique();
    }
}
