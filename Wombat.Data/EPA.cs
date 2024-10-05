using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wombat.Data
{
    public class EPA: BaseEntity
    {
        public EPA()
        {
            Name = "";
            Description = "";
            Templates = new List<AssessmentTemplate>();
        }

        public string Description { get; set; }

        public string Name { get; set; }

        public List<AssessmentTemplate> Templates { get; set; }
    }
}
