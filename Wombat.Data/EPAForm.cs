using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Data
{
    public class EPAForm: BaseEntity
    {
        public int EPAId { get; set; }

        public required EPA EPA { get; set; }

        public int FormId { get; set; }

        public required AssessmentForm Form { get; set; }
    }
}
