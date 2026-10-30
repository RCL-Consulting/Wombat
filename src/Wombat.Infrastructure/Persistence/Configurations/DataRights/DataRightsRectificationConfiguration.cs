using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.DataRights;

namespace Wombat.Infrastructure.Persistence.Configurations.DataRights;

public sealed class DataRightsRectificationConfiguration : IEntityTypeConfiguration<DataRightsRectification>
{
    public void Configure(EntityTypeBuilder<DataRightsRectification> builder)
    {
        builder.ToTable("DataRightsRectifications");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("uuid")
            .ValueGeneratedNever();

        builder.Property(e => e.RequestId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(e => e.TargetType)
            .HasMaxLength(200)
            .IsRequired();

        // T387 (Q26): the record is always the requester's, so the request says whose; TargetId (a Guid, which could not hold
        // a trainee profile's int id) is gone. Who recorded it is kept (S11), as the request keeps who decided it.
        builder.Property(e => e.AppliedByUserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(e => e.FromValueJson)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(e => e.ToValueJson)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(e => e.AppliedOn)
            .HasColumnType("timestamp with time zone")
            .IsRequired();
    }
}
