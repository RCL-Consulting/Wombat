using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Common.Models
{
    public class EPAFormVM : Collection
    {
        public int EPAId { get; set; }
        public EPAVM? EPA { get; set; }

        public int FormId { get; set; }
        public AssessmentFormVM? Form { get; set; }
    }
}
