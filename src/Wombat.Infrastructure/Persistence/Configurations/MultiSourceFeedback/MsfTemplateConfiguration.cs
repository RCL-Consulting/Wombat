using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Infrastructure.Persistence.Configurations.MultiSourceFeedback;

public sealed class MsfTemplateConfiguration : IEntityTypeConfiguration<MsfTemplate>
{
    public void Configure(EntityTypeBuilder<MsfTemplate> builder)
    {
        builder.ToTable("MsfTemplates");
        builder.Property(entity => entity.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(entity => entity.Name);
    }
}
