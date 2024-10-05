using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wombat.Data.EPA
{
    public class Competency : BaseEntity
    {
        public Competency()
        {
            Description = "";
            Code = "";
            DisplayRank = true;
            Milestones = new List<Milestone>();
        }

        public string Description { get; set; }

        public string Code { get; set; }

        public bool DisplayRank { get; set; }

        public List<Milestone> Milestones { get; set; }
    }
}
