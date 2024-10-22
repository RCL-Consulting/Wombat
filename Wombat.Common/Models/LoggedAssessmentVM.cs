namespace Wombat.Common.Models
{
    public class LoggedAssessmentVM
    {
        public LoggedAssessmentVM()
        {
            OptionCriterionResponses = new List<OptionCriterionResponseVM>();
            TraineeId = "";
            AssessorId = "";
            EPAId = 0;
        }

        public bool AssessmentIsPublic { get; set; } = false;
        public int Id { get; set; }
        public string TraineeId { get; set; }
        public WombatUserVM? Trainee { get; set; }

        public string AssessorId { get; set; }
        public WombatUserVM? Assessor { get; set; }

        public int EPAId { get; set; }
        public EPAVM? EPA { get; set; }

        public int FormId { get; set; }
        public AssessmentFormVM? Form { get; set; }

        public int AssessmentRequestId { get; set; }

        public List<OptionCriterionResponseVM> OptionCriterionResponses { get; set; }

        public DateTime AssessmentDate { get; set; }

        public int Score { get; set; }

        public void SetScore()
        {
            int Result = 0;
            foreach (var item in OptionCriterionResponses)
            {
                if (item.Criterion.OptionSetId == 2)
                {
                    Result = item.Option.Rank;
                    break;
                }
            }
            Score = Result;
        }
    }
}
