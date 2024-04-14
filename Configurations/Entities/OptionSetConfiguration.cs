using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Data;

namespace Wombat.Configurations.Entities
{
    public class OptionSetConfiguration : IEntityTypeConfiguration<OptionSet>
    {
        public void Configure(EntityTypeBuilder<OptionSet> builder)
        {
            builder.HasData(
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
            /*3		False	0001/01/01 00:00:00	0001/01/01 00:00:00
4	Difficulty level (3)	True	0001/01/01 00:00:00	0001/01/01 00:00:00
5	Skill level (5)	True	0001/01/01 00:00:00	0001/01/01 00:00:00
6	Research progress	False	0001/01/01 00:00:00	0001/01/01 00:00:00
7	Research project progress	False	0001/01/01 00:00:00	0001/01/01 00:00:00*/
        }
    }
}
