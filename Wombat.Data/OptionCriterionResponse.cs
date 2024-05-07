namespace Wombat.Data
{
    public class OptionCriterionResponse: BaseEntity
    {
        public OptionCriterionResponse()
        {
            Comment = "";
        }
        public int OptionId { get; set; }
        public Option? Option { get; set; }

        public int CriterionId { get; set; }
        public OptionCriterion? Criterion { get; set; }

        public int AssessmentId { get; set; }
        public LoggedAssessment? Assessment { get; set; }

        public string Comment { get; set; }
    }
}
