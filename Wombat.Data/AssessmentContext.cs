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

        [ForeignKey("AssessmentCategoryId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public AssessmentCategory? AssessmentCategory { get; set; }

        public int AssessmentCategoryId { get; set; }
    }
}
