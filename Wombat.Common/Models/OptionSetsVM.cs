namespace Wombat.Common.Models
{
    public class OptionSetsVM
    {
        public OptionCriterionVM OptionCriterion { get; set; }
        public int OptionSetId { get; set; }
        public List<OptionSetVM> OptionSets { get; set; }
    }
}
