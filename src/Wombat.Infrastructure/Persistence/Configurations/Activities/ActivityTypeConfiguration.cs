using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Activities;

namespace Wombat.Infrastructure.Persistence.Configurations.Activities;

public sealed class ActivityTypeConfiguration : IEntityTypeConfiguration<ActivityType>
{
    public void Configure(EntityTypeBuilder<ActivityType> builder)
    {
        builder.ToTable("ActivityTypes");
        builder.Property(entity => entity.Key).HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.Name).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(2000);
        builder.Property(entity => entity.OwnerUserId).HasMaxLength(450).IsRequired();
        builder.Property(entity => entity.SchemaJson).HasColumnType("jsonb");
        builder.Property(entity => entity.WorkflowJson).HasColumnType("jsonb");
        builder.Property(entity => entity.CreditRulesJson).HasColumnType("jsonb");
        builder.Property(entity => entity.DisplayFieldsJson).HasColumnType("jsonb").IsRequired();
        builder.Property(entity => entity.StagingSchemaJson).HasColumnType("jsonb");
        builder.Property(entity => entity.StagingWorkflowJson).HasColumnType("jsonb");
        builder.Property(entity => entity.StagingCreditRulesJson).HasColumnType("jsonb");
        builder.Property(entity => entity.StagingDisplayFieldsJson).HasColumnType("jsonb");
        builder.Property(entity => entity.StagingUpdatedByUserId).HasMaxLength(450);
        builder.Property(entity => entity.StagingUpdatedOn).HasColumnType("timestamp with time zone");
        builder.Property(entity => entity.CreatedOn).HasColumnType("timestamp with time zone");

        builder.HasIndex(entity => entity.Key).IsUnique();
        builder.HasIndex(entity => new { entity.Scope, entity.ScopeId });

        builder.HasMany(entity => entity.PermissionRules)
            .WithOne(entity => entity.ActivityType)
            .HasForeignKey(entity => entity.ActivityTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(entity => entity.Activities)
            .WithOne(entity => entity.ActivityType)
            .HasForeignKey(entity => entity.ActivityTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(entity => entity.Versions)
            .WithOne(entity => entity.ActivityType)
            .HasForeignKey(entity => entity.ActivityTypeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
