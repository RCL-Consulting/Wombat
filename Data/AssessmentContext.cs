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
        public AssessmentCategory? AssessmentCategory { get; set; }
        public int AssessmentCategoryId { get; set; }
    }
}
