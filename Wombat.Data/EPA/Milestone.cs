using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wombat.Data.EPA
{
    public class Milestone : BaseEntity
    {
        public Milestone()
        {
            Description = "";
        }
        public string Description { get; set; }
        public int Rank { get; set; }

        public int CompetencyId { get; set; }

        [ForeignKey("CompetencyId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public Competency? Competency { get; set; }

    }
}
