using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Wombat.Data;

namespace Wombat.Configurations.Entities
{
    public class OptionConfiguration : IEntityTypeConfiguration<Option>
    {
        public void Configure(EntityTypeBuilder<Option> builder)
        {
            builder.HasData(
                new Option
                {
                    Id = 10,
                    Description = "Ward",
                    Rank = 0,
                    OptionSetId = 3
                },
                new Option
                {
                    Id = 11,
                    Description = "Clinic",
                    Rank = 1,
                    OptionSetId = 3
                },
                new Option
                {
                    Id = 12,
                    Description = "Intensive care unit",
                    Rank = 2,
                    OptionSetId = 3
                },
                new Option
                {
                    Id = 13,
                    Description = "Other",
                    Rank = 3,
                    OptionSetId = 3
                },
                new Option
                {
                    Id = 14,
                    Description = "Low",
                    Rank = 0,
                    OptionSetId = 4
                },
                new Option
                {
                    Id = 15,
                    Description = "Moderate",
                    Rank = 1,
                    OptionSetId = 4
                },
                new Option
                {
                    Id = 16,
                    Description = "High",
                    Rank = 2,
                    OptionSetId = 4
                },
                new Option
                {
                    Id = 17,
                    Description = "Not observed",
                    Rank = 0,
                    OptionSetId = 5
                },
                new Option
                {
                    Id = 18,
                    Description = "Requires intervention",
                    Rank = 1,
                    OptionSetId = 5
                },
                new Option
                {
                    Id = 19,
                    Description = "Room for improvement, still requires supervision",
                    Rank = 2,
                    OptionSetId = 5
                },
                new Option
                {
                    Id = 20,
                    Description = "Adequate - can do this unsupervised",
                    Rank = 3,
                    OptionSetId = 5
                },
                new Option
                {
                    Id = 21,
                    Description = "Good enough to train a junior colleague",
                    Rank = 4,
                    OptionSetId = 5
                },
                new Option
                {
                    Id = 22,
                    Description = "Topic selected",
                    Rank = 0,
                    OptionSetId = 6
                },
                new Option
                {
                    Id = 23,
                    Description = "Protocol development for MMed review committee",
                    Rank = 1,
                    OptionSetId = 6
                },
                new Option
                {
                    Id = 24,
                    Description = "Protocol reviews after MMed committee submission",
                    Rank = 2,
                    OptionSetId = 6
                },
                new Option
                {
                    Id = 25,
                    Description = "Protocol submission for Ethics committee",
                    Rank = 3,
                    OptionSetId = 6
                },
                new Option
                {
                    Id = 26,
                    Description = "Protocol reviews after Ethics committee submission",
                    Rank = 4,
                    OptionSetId = 6
                },
                new Option
                {
                    Id = 27,
                    Description = "Data collection",
                    Rank = 5,
                    OptionSetId = 6
                },
                new Option
                {
                    Id = 28,
                    Description = "Data analysis",
                    Rank = 6,
                    OptionSetId = 6
                },
                new Option
                {
                    Id = 29,
                    Description = "Final write up",
                    Rank = 7,
                    OptionSetId = 6
                },
                new Option
                {
                    Id = 30,
                    Description = "Satisfactory",
                    Rank = 0,
                    OptionSetId = 7
                },
                new Option
                {
                    Id = 31,
                    Description = "Needs intervention",
                    Rank = 1,
                    OptionSetId = 7
                }                
            );
        }
    }
}
