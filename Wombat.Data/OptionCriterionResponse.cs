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
using System.ComponentModel.DataAnnotations.Schema;

namespace Wombat.Data
{
    public class OptionCriterionResponse: BaseEntity
    {
        public OptionCriterionResponse()
        {
            Comment = "";
        }
        public int? OptionId { get; set; }

        [ForeignKey("OptionId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public Option? Option { get; set; }

        public int CriterionId { get; set; }

        [ForeignKey("CriterionId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public OptionCriterion? Criterion { get; set; }

        public int AssessmentId { get; set; }

        [ForeignKey("AssessmentId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public LoggedAssessment? Assessment { get; set; }

        public string Comment { get; set; }
    }
}
