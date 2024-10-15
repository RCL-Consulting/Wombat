using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wombat.Common.Models;

namespace Wombat.Data.Configurations.Entities
{
    public class OptionCriterionConfiguration : IEntityTypeConfiguration<OptionCriterion>
    {
        public void Configure(EntityTypeBuilder<OptionCriterion> builder)
        {
            builder.HasData(
                new OptionCriterion
                {
                    Id = 1,
                    Description = "Assessment rating",
                    OptionSetId = 2,
                    Rank = 1000,
                    AssessmentFormId = 1
                },
                new OptionCriterion
                {
                    Id = 2,
                    Description = "Briefly state at least one observation that supports the EPA rating you assigned",
                    OptionSetId = 1,
                    Rank = 1001,
                    AssessmentFormId = 1
                },
                new OptionCriterion
                {
                    Id = 3,
                    Description = "Briefly state at least one thing that needs to be demonstrated by the trainee to advance the EPA rating to the next level",
                    OptionSetId = 1,
                    Rank = 1002,
                    AssessmentFormId = 1
                }
            );
        }
    }
}
