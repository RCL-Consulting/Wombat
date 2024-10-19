using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Common.Models
{
    public class AssessmentRequestVM
    {
        int Id { get; set; }

        DateTime DateRequested { get; set; }
        DateTime? DateAccepted { get; set; }
        DateTime AssessmentDate { get; set; }

        public WombatUserVM? Trainee { get; set; }
        public WombatUserVM? Assessor { get; set; }

        public EPAVM? EPA { get; set; }
        public AssessmentFormVM? AssessmentForm { get; set; }
    }
}
