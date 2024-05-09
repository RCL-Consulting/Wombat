namespace Wombat.Data
{
    public class AssessmentCategory: BaseEntity
    {
        public string Name { get; set; }

        public List<OptionCriterion> OptionCriteria { get; set; }

        public AssessmentCategory()
        {
            this.OptionCriteria = new List<OptionCriterion>();
            Name = "";
        }
    }
}
