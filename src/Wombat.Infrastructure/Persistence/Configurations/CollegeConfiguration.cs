using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Institutions;

namespace Wombat.Infrastructure.Persistence.Configurations;

public sealed class CollegeConfiguration : IEntityTypeConfiguration<College>
{
    public void Configure(EntityTypeBuilder<College> builder)
    {
        builder.ToTable("Colleges");
        builder.Property(entity => entity.Name).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.ShortCode).HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(1000);
        builder.HasIndex(entity => entity.Name).IsUnique();
        builder.HasIndex(entity => entity.ShortCode).IsUnique();
    }
}
