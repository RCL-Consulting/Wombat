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

        // T101: read authorization filters lists by the activity's own scope, so these are read-path
        // indexes, not FKs. No foreign key is declared deliberately — the columns are a snapshot of
        // where the subject trained at creation, and a later restructure of the institution tree must
        // not cascade into, or be blocked by, historical assessments.
        builder.HasIndex(entity => entity.InstitutionId);
        builder.HasIndex(entity => entity.SpecialityId);
        builder.HasIndex(entity => entity.SubSpecialityId);

        builder.HasMany(entity => entity.Transitions)
            .WithOne(entity => entity.Activity)
            .HasForeignKey(entity => entity.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
