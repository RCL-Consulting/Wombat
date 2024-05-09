using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wombat.Data
{
    public class Option : BaseEntity
    {
        public Option()
        {
            Description = "";
        }
        public string Description { get; set; }
        public int Rank { get; set; }

        public int OptionSetId { get; set; }

        [ForeignKey("OptionSetId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public OptionSet? OptionSet { get; set; }

    }
}
