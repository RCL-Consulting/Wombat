using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Infrastructure.Persistence.Configurations.MultiSourceFeedback;

public sealed class MsfQuestionConfiguration : IEntityTypeConfiguration<MsfQuestion>
{
    public void Configure(EntityTypeBuilder<MsfQuestion> builder)
    {
        builder.ToTable("MsfQuestions");
        builder.Property(entity => entity.Prompt).HasMaxLength(1000).IsRequired();
        builder.HasIndex(entity => new { entity.TemplateId, entity.Order }).IsUnique();

        builder.HasOne(entity => entity.Template)
            .WithMany(template => template.Questions)
            .HasForeignKey(entity => entity.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
