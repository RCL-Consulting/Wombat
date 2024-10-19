using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wombat.Common.Models;

namespace Wombat.Data
{
    public class AssessmentRequest: BaseEntity
    {
        DateTime DateRequested { get; set; }

        DateTime? DateAccepted { get; set; }
        DateTime AssessmentDate { get; set; }

        string TraineeId { get; set; }

        [ForeignKey("TraineeId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public WombatUser? Trainee{ get; set; }

        string AssessorId { get; set; }

        [ForeignKey("AssessorId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public WombatUser? Assessor { get; set; }

        int EPAId { get; set; }

        [ForeignKey("EPAId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public EPA? EPA { get; set; }

        int AssessmentFormId { get; set; }

        [ForeignKey("AssessmentFormId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public AssessmentForm? AssessmentForm { get; set; }
    }
}
