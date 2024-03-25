using System.ComponentModel.DataAnnotations.Schema;

namespace Wombat.Data
{
    public class Assessment: BaseEntity
    {
        public string TraineeId { get; set; }

        public string AssessorId { get; set; }

        [ForeignKey("AssessmentCategoryId")]
        public AssessmentCategory AssessmentCategory { get; set; }
        public int AssessmentCategoryId { get; set; }
    }
}
