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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Data.Configurations.Entities
{
    public class AssessmentFormConfiguration : IEntityTypeConfiguration<AssessmentForm>
    {
        public void Configure(EntityTypeBuilder<AssessmentForm> builder)
        {
            builder.HasData(
                new AssessmentForm
                {
                    Id = 1,
                    Name = "Default Template",
                    CanDelete = false
                },
                new AssessmentForm
                {
                    Id = 2,
                    Name = "Paediatrics-General EPA1 CEX",
                    CanDelete = true
                },
                new AssessmentForm
                {
                    Id = 3,
                    Name = "Paediatrics-General EPA2 CEX",
                    CanDelete = true
                },
                new AssessmentForm
                {
                    Id = 4,
                    Name = "Paediatrics-General EPA3 CEX",
                    CanDelete = true
                },
                new AssessmentForm
                {
                    Id = 5,
                    Name = "Paediatrics-General EPA4 CEX",
                    CanDelete = true
                }
            );
        }
    }
}
