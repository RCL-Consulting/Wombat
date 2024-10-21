using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Common.Models
{
    public class AssessmentRequestVM
    {
        public int Id { get; set; }

        [Display(Name = "Trainee")]
        public string TraineeDisplayName { get { return Trainee?.Name + " " + Trainee?.Surname + " (" + Trainee?.Email + ")"; } }

        [Display(Name = "EPA")]
        public string FullEPADisplayName { get { return EPA?.Name + " - " + EPA?.Description; } }

        public string AssessorNotes { get; set; } = "";

        [Display(Name = "Date requested")]
        public DateTime? DateRequested { get; set; }

        [Display(Name = "Date accepted")]
        public DateTime? DateAccepted { get; set; }

        [Display(Name = "Date declined")]
        public DateTime? DateDeclined { get; set; }

        [Display(Name = "Assessment date")]
        public DateTime? AssessmentDate { get; set; }

        public string TraineeId { get; set; }
        public WombatUserVM? Trainee { get; set; }

        public string AssessorId { get; set; }
        public WombatUserVM? Assessor { get; set; }

        public int EPAId { get; set; }
        public EPAVM? EPA { get; set; }

        public int? AssessmentFormId { get; set; }
        public AssessmentFormVM? AssessmentForm { get; set; }

        public string Notes { get; set; } = "";
    }
}
