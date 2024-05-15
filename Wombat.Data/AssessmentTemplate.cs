namespace Wombat.Data
{
    public class AssessmentTemplate : BaseEntity
    {
        public string Name { get; set; }

        public List<OptionCriterion> OptionCriteria { get; set; }

        public AssessmentTemplate()
        {
            this.OptionCriteria = new List<OptionCriterion>();
            Name = "";
        }
    }
}
