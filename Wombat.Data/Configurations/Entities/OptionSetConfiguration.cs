using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Wombat.Data.Configurations.Entities
{
    public class OptionSetConfiguration : IEntityTypeConfiguration<OptionSet>
    {
        public void Configure(EntityTypeBuilder<OptionSet> builder)
        {
            builder.HasData(
                new OptionSet
                {
                    Id = 1,
                    Description = "Text",
                    DisplayRank=false
                },
                new OptionSet
                {
                    Id = 3,
                    Description = "Hospital locations",
                    DisplayRank=false
                },
               new OptionSet
               {
                   Id = 4,
                   Description = "Difficulty level (3)",
                   DisplayRank=true
               },
               new OptionSet
               {
                   Id = 5,
                   Description = "Skill level (5)",
                   DisplayRank=true
               },
               new OptionSet
               {
                   Id = 6,
                   Description = "Research progress",
                   DisplayRank=false
               },
               new OptionSet
               {
                   Id = 7,
                   Description = "Research project progress",
                   DisplayRank=false
               }
            );
        }
    }
}
