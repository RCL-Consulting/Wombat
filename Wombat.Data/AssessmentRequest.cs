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
using Wombat.Common.Models;

namespace Wombat.Data
{
    public class AssessmentRequest: BaseEntity
    {
        public string? AssessorNotes { get; set; }
        public DateTime? DateRequested { get; set; }

        public DateTime? DateAccepted { get; set; }
        public DateTime? DateDeclined { get; set; }
        public DateTime? AssessmentDate { get; set; }
        public DateTime? CompletionDate { get; set; }

        public string TraineeId { get; set; } = "";

        [ForeignKey("TraineeId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public WombatUser? Trainee{ get; set; }

        public string AssessorId { get; set; } = "";

        [ForeignKey("AssessorId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public WombatUser? Assessor { get; set; }

        public int EPAId { get; set; }

        [ForeignKey("EPAId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public EPA? EPA { get; set; }

        public int AssessmentFormId { get; set; }

        [ForeignKey("AssessmentFormId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public AssessmentForm? AssessmentForm { get; set; }

        public string Notes { get; set; } = "";
    }
}
