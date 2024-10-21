using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wombat.Data
{
    public class LoggedAssessment: BaseEntity
    {
        public LoggedAssessment()
        {
            GoodComment = "";
            BadComment = "";
        }

        public string TraineeId { get; set; }

        [ForeignKey("TraineeId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public WombatUser? Trainee { get; set; }

        public string AssessorId { get; set; }

        [ForeignKey("AssessorId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public WombatUser? Assessor { get; set; }

        public int EPAId { get; set; }

        [ForeignKey("EPAId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public EPA? EPA { get; set; }

        public int FormId { get; set; }
        [ForeignKey("FormId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public AssessmentForm? Form { get; set; }

        public int AssessmentRequestId { get; set; }
        [ForeignKey("AssessmentRequestId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public AssessmentForm? AssessmentRequest { get; set; }

        public List<OptionCriterionResponse>? OptionCriterionResponses { get; set; }

        public string GoodComment { get; set; }
        public string BadComment { get; set; }

        public DateTime AssessmentDate { get; set; }

    }
}
