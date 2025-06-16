using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Data
{
    public class STARApplicationForm : BaseEntity
    {
        public string Name { get; set; } = "";
        public int EPAId { get; set; }

        [ForeignKey(nameof(EPAId))]
        public EPA? EPA { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public bool IsPublished { get; set; } = false;

        public ICollection<STARItem> STARItems { get; set; } = new List<STARItem>();
    }
}
