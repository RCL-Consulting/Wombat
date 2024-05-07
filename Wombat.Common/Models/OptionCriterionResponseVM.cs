namespace Wombat.Common.Models
{
    public class OptionCriterionResponseVM
    {
        public OptionCriterionResponseVM()
        {
            Comment = "";
        }

        public int Id { get; set; }

        public OptionVM? Option { get; set; }
        public int OptionId { get; set; }

        public int CriterionId { get; set; }
        public OptionCriterionVM? Criterion { get; set; }

        public int AssessmentId { get; set; }
        public LoggedAssessmentVM? Assessment { get; set; }

        public string? Comment { get; set; }
    }
}
