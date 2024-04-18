namespace Wombat.Data
{
    public class OptionCriterionResponse: BaseEntity
    {
        public string LoggedOptionId { get; set; }
        public Option Option { get; set; }

        public int OptionCriterionId { get; set; }
        public OptionCriterion OptionCriterion { get; set; }

        public int AssessmentId { get; set; }
        public LoggedAssessment LoggedAssessment { get; set; }

        public string Comment { get; set; }
    }
}
