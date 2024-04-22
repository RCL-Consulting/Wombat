using Wombat.Data;

namespace Wombat.Models
{
    public class LoggedAssessmentVM
    {
        public LoggedAssessmentVM()
        {
            OptionCriterionResponses = new List<OptionCriterionResponseVM>();
            TraineeId = "";
            AssessorId = "";
            AssessmentContextId = 0;
            Comment = "";
        }

        public int Id { get; set; }
        public string TraineeId { get; set; }
        public WombatUser? Trainee { get; set; }

        public string AssessorId { get; set; }
        public WombatUser? Assessor { get; set; }

        public int AssessmentContextId { get; set; }
        public AssessmentContextVM? AssessmentContext { get; set; }

        public List<OptionCriterionResponseVM> OptionCriterionResponses { get; set; }

        public string Comment { get; set; }

        public DateTime AssessmentDate { get; set; }
    }
}
