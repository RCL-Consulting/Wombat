namespace Wombat.Data
{
    public class OptionCriterion: BaseEntity
    {
        public OptionCriterion()
        {
            Description = "";
        }
        public string Description { get; set; }

        public int OptionSetId { get; set; }
        public OptionSet? OptionsSet { get; set; }

        public int AssessmentCategoryId { get; set; }
        public AssessmentCategory? AssessmentCategory { get; set; }

        public int Rank { get; set; }
    }
}
