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
                    DisplayRank=false,
                    CanDelete = false,
                    CanEdit = false
                },
                new OptionSet
                {
                    Id = 2,
                    Description = "EPA scale",
                    DisplayRank = true,
                    CanDelete = false
                },
                new OptionSet
                {
                    Id = 3,
                    Description = "CEX scale",
                    DisplayRank = true,
                }
            );
        }
    }
}
