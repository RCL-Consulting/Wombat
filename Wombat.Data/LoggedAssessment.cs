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
    public class LoggedAssessment: BaseEntity
    {
        public LoggedAssessment()
        {
        }

        public bool AssessmentIsPublic { get; set; } = false;
        public string TraineeId { get; set; }

        [ForeignKey("TraineeId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public WombatUser? Trainee { get; set; }

        public string AssessorId { get; set; }

        [ForeignKey("AssessorId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public WombatUser? Assessor { get; set; }

        public int EPAId { get; set; }

        [ForeignKey("EPAId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public EPA? EPA { get; set; }

        public int FormId { get; set; }
        [ForeignKey("FormId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public AssessmentForm? Form { get; set; }

        public int? AssessmentRequestId { get; set; }
        [ForeignKey("AssessmentRequestId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public AssessmentRequest? AssessmentRequest { get; set; }

        public List<OptionCriterionResponse>? OptionCriterionResponses { get; set; }

        public DateTime AssessmentDate { get; set; }

    }
}
