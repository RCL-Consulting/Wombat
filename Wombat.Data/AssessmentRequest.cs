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
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wombat.Common.Constants;

namespace Wombat.Data
{
    public class AssessmentRequest : BaseEntity
    {
        // Single source of truth for lifecycle
        public AssessmentRequestStatus Status { get; set; } = AssessmentRequestStatus.Requested;
        public DateTime StatusChangedAt { get; set; } = DateTime.UtcNow;

        // Domain dates (not statuses)
        public DateTime? AssessmentDate { get; set; }       // scheduled date/time
        public DateTime? CompletionDate { get; set; }       // when assessment was logged

        // Parties / references
        public string TraineeId { get; set; } = "";
        [ForeignKey(nameof(TraineeId)), DeleteBehavior(DeleteBehavior.Restrict)]
        public WombatUser? Trainee { get; set; }

        public string AssessorId { get; set; } = "";
        [ForeignKey(nameof(AssessorId)), DeleteBehavior(DeleteBehavior.Restrict)]
        public WombatUser? Assessor { get; set; }

        public int EPAId { get; set; }
        [ForeignKey(nameof(EPAId)), DeleteBehavior(DeleteBehavior.Restrict)]
        public EPA? EPA { get; set; }

        public int AssessmentFormId { get; set; }
        [ForeignKey(nameof(AssessmentFormId)), DeleteBehavior(DeleteBehavior.Restrict)]
        public AssessmentForm? AssessmentForm { get; set; }

        public LoggedAssessment? LoggedAssessment { get; set; }
    }
}
