using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Wombat.Data.Configurations.Entities
{
    public class OptionConfiguration : IEntityTypeConfiguration<Option>
    {
        public void Configure(EntityTypeBuilder<Option> builder)
        {
            builder.HasData(
               
                new Option
                {
                    Id = 1,
                    Description = "Not observed",
                    Rank = 0,
                    OptionSetId = 2
                },
                new Option
                {
                    Id = 2,
                    Description = "Requires intervention",
                    Rank = 1,
                    OptionSetId = 2
                },
                new Option
                {
                    Id = 3,
                    Description = "Room for improvement, still requires supervision",
                    Rank = 2,
                    OptionSetId = 2
                },
                new Option
                {
                    Id = 4,
                    Description = "Adequate - can do this unsupervised",
                    Rank = 3,
                    OptionSetId = 2
                },
                new Option
                {
                    Id = 5,
                    Description = "Good enough to train a junior colleague",
                    Rank = 4,
                    OptionSetId = 2
                },
                new Option
                {
                    Id = 6,
                    Description = "Not yet",
                    Rank = 0,
                    OptionSetId = 3
                },
                new Option
                {
                    Id = 7,
                    Description = "At times, but not consistently",
                    Rank = 1,
                    OptionSetId = 3
                },
                new Option
                {
                    Id = 8,
                    Description = "Consistently",
                    Rank = 2,
                    OptionSetId = 3
                },
                new Option
                {
                    Id = 9,
                    Description = "Unable to assess",
                    Rank = -1,
                    OptionSetId = 3
                }
            );
        }
    }
}
