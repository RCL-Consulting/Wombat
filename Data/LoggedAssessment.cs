using Wombat.Models;

namespace Wombat.Data
{
    public class LoggedAssessment: BaseEntity
    {
        public string TraineeId { get; set; }
        public WombatUser? Trainee { get; set; }

        public string AssessorId { get; set; }
        public WombatUser? Assessor { get; set; }

        public int AssessmentContextId { get; set; }
        public AssessmentContext? AssessmentContext { get; set; }

        public List<OptionCriterionResponse>? OptionCriterionResponses { get; set; }

        public string Comment { get; set; }

        public DateTime AssessmentDate { get; set; }

    }
}
