/*Copyright (C) 2024 RCL Consulting
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>
 */

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
