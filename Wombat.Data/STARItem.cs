using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Data
{
    public class STARItem : BaseEntity
    {
        public string Heading { get; set; } = "";
        public string Description { get; set; } = "";

        public int OptionSetId { get; set; }

        [ForeignKey(nameof(OptionSetId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public OptionSet? OptionsSet { get; set; }

        public int FormId { get; set; }

        [ForeignKey(nameof(FormId))]
        public STARApplicationForm? Form { get; set; }

        public int Rank { get; set; }
    }
}
