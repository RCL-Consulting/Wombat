namespace Wombat.Data
{
    public class AssessmentCategory: BaseEntity
    {
        public string Name { get; set; }

        public List<OptionCriterion> OptionCriteria { get; set; }
        public List<TextCriterion> TextCriteria { get; set; }

        public AssessmentCategory()
        {
            this.OptionCriteria = new List<OptionCriterion>();
            this.TextCriteria = new List<TextCriterion>();
            Name = "";
        }
    }
}
