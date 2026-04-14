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

        builder.Property(e => e.TargetId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(e => e.FromValueJson)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(e => e.ToValueJson)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(e => e.AppliedOn)
            .HasColumnType("timestamp with time zone");
    }
}
