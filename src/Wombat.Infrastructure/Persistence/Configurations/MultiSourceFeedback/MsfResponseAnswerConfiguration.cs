using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Infrastructure.Persistence.Configurations.MultiSourceFeedback;

public sealed class MsfResponseAnswerConfiguration : IEntityTypeConfiguration<MsfResponseAnswer>
{
    public void Configure(EntityTypeBuilder<MsfResponseAnswer> builder)
    {
        builder.ToTable("MsfResponseAnswers");
        builder.Property(entity => entity.LongText).HasMaxLength(4000);
        builder.HasIndex(entity => new { entity.ResponseId, entity.QuestionId }).IsUnique();

        builder.HasOne(entity => entity.Response)
            .WithMany(response => response.Answers)
            .HasForeignKey(entity => entity.ResponseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(entity => entity.Question)
            .WithMany()
            .HasForeignKey(entity => entity.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
