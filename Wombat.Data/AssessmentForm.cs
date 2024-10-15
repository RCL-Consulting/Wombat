using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wombat.Data
{
    public class AssessmentForm : BaseEntity
    {
        public string Name { get; set; }

        public List<OptionCriterion> OptionCriteria { get; set; }

        public AssessmentForm()
        {
            this.OptionCriteria = new List<OptionCriterion>();
            Name = "";
        }

        public ICollection<EPAForm> EPAs { get; set; }

        public bool CanDelete { get; set; } = true;

        public const int kTemplateId = 1;
    }
}
