using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Common.Models
{
    public class DashboardVM
    {
        public WombatUserVM User { get; set; }

        public List<EPAVM> EPAList { get; set; }

        [Display(Name = "Declined requests")]
        public int NumberOfRequestsDeclined { get; set; } = 0;

        [Display(Name = "Pending requests")]
        public int NumberOfRequestsMade { get; set; } = 0;

        [Display(Name = "Pending assessments")]
        public int NumberOfPendingAssessments { get; set; } = 0;

        [Display(Name = "Completed assessments")]
        public int NumberOfCompletedAssessments { get; set; } = 0;
    }
}
