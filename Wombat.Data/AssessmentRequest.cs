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
