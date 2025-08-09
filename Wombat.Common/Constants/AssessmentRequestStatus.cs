using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Common.Constants
{
    public enum AssessmentRequestStatus
    {
        [Display(Name = "Requested")]
        Requested,
        [Display(Name = "Accepted")]
        Accepted,
        [Display(Name = "Declined")]
        Declined,
        [Display(Name = "Cancelled")]
        Cancelled,
        [Display(Name = "Completed")]
        Completed,
        [Display(Name = "Not conducted")]
        NotConducted,
        [Display(Name = "Expired")]
        Expired,
        [Display(Name = "Awaiting assessment")]
        None
    }
}
