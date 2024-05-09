using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wombat.Data
{
    public class OptionCriterionResponse: BaseEntity
    {
        public OptionCriterionResponse()
        {
            Comment = "";
        }
        public int? OptionId { get; set; }

        [ForeignKey("OptionId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public Option? Option { get; set; }

        public int CriterionId { get; set; }

        [ForeignKey("CriterionId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public OptionCriterion? Criterion { get; set; }

        public int AssessmentId { get; set; }

        [ForeignKey("AssessmentId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public LoggedAssessment? Assessment { get; set; }

        public string Comment { get; set; }
    }
}
