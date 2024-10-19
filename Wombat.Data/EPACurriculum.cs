using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Data
{
    public class EPACurriculum : BaseEntity
    {
        public EPACurriculum()
        {
        }
        
        public int NumberOfMonths { get; set; }

        public int EPAId { get; set; }

        [ForeignKey("EPAId")]
        [DeleteBehavior(DeleteBehavior.Cascade)]
        public EPA? EPA { get; set; }

        public int EPAScaleId { get; set; }

        [ForeignKey("EPAScaleId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public Option? EPAScaleOption { get; set; }

    }
}
