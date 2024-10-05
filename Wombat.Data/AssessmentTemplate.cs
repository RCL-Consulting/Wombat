using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wombat.Data
{
    public class AssessmentTemplate : BaseEntity
    {
        public string Name { get; set; }

        public List<OptionCriterion> OptionCriteria { get; set; }

        public AssessmentTemplate()
        {
            this.OptionCriteria = new List<OptionCriterion>();
            Name = "";
        }

        public int? EPAId { get; set; }

        [ForeignKey("EPAId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public EPA? EPA { get; set; }
    }
}
