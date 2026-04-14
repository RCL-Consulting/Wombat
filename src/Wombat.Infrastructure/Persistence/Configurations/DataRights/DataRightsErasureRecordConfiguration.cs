using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.DataRights;

namespace Wombat.Infrastructure.Persistence.Configurations.DataRights;

public sealed class DataRightsErasureRecordConfiguration : IEntityTypeConfiguration<DataRightsErasureRecord>
{
    public void Configure(EntityTypeBuilder<DataRightsErasureRecord> builder)
    {
        builder.ToTable("DataRightsErasureRecords");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("uuid")
            .ValueGeneratedNever();

        builder.Property(e => e.RequestId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(e => e.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(e => e.Pseudonym)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.ErasedOn)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(e => e.RetentionReasonsJson)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.HasOne(e => e.Request)
            .WithMany()
            .HasForeignKey(e => e.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.UserId).IsUnique();
        builder.HasIndex(e => e.Pseudonym).IsUnique();
    }
}
