using Wombat.Data;

namespace Wombat.Models
{
    public class LoggedAssessmentVM
    {
        public int Id { get; set; }
        public string TraineeId { get; set; }
        public WombatUser Trainee { get; set; }

        public string AssessorId { get; set; }
        public WombatUser Assessor { get; set; }

        public int AssessmentContextId { get; set; }
        public AssessmentContext AssessmentContext { get; set; }

        public DateTime AssessmentDate { get; set; }
    }
}
