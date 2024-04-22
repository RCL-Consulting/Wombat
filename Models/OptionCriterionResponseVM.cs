using Wombat.Data;

namespace Wombat.Models
{
    public class OptionCriterionResponseVM
    {
        public OptionCriterionResponseVM()
        {
            Comment = "";
        }

        public int Id { get; set; }

        public Option? Option { get; set; }
        public int OptionId { get; set; }

        public int CriterionId { get; set; }
        public OptionCriterionVM? Criterion { get; set; }

        public int AssessmentId { get; set; }
        public LoggedAssessment? Assessment { get; set; }

        public string Comment { get; set; }
    }
}
