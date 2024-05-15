using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wombat.Data
{
    public class AssessmentContext: BaseEntity
    {
        public AssessmentContext()
        {
            Description = "";
        }

        public string Description { get; set; }

        [ForeignKey("AssessmentTemplateId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public AssessmentTemplate? AssessmentTemplate { get; set; }

        public int AssessmentTemplateId { get; set; }
    }
}
