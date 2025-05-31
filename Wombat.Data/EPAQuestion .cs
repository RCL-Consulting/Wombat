using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Data
{
    public class EPAQuestion : BaseEntity
    {
        public string Heading { get; set; } = "";
        public string Description { get; set; } = "";

        public int OptionSetId { get; set; }

        [ForeignKey("OptionSetId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public OptionSet? OptionsSet { get; set; }

        public int EPAId { get; set; }

        [ForeignKey("EPAId")]
        [DeleteBehavior(DeleteBehavior.Cascade)]
        public EPA? EPA { get; set; }

        public int Rank { get; set; }
    }
}
