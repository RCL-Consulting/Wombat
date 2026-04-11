using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Activities;

namespace Wombat.Infrastructure.Persistence.Configurations.Activities;

public sealed class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        builder.ToTable("Activities");
        builder.Property(entity => entity.SubjectUserId).HasMaxLength(450).IsRequired();
        builder.Property(entity => entity.CreatedByUserId).HasMaxLength(450).IsRequired();
        builder.Property(entity => entity.CurrentState).HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.DataJson).HasColumnType("jsonb").IsRequired();
        builder.Property(entity => entity.CreatedOn).HasColumnType("timestamp with time zone");
        builder.Property(entity => entity.UpdatedOn).HasColumnType("timestamp with time zone");

        builder.HasIndex(entity => new { entity.ActivityTypeId, entity.CurrentState, entity.SubjectUserId });
        builder.HasIndex(entity => entity.SubjectUserId);
        builder.HasIndex(entity => entity.CreatedOn);
        builder.HasIndex(entity => entity.DataJson).HasMethod("gin");

        builder.HasMany(entity => entity.Transitions)
            .WithOne(entity => entity.Activity)
            .HasForeignKey(entity => entity.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
