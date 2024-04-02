namespace Wombat.Data
{
    public class EnumOption: BaseEntity
    {
        public string Description { get; set; }
        public int Rank { get; set;}

        public int EnumCriterionId { get; set; }
        public EnumCriterion EnumCriterion { get; set; }

    }
}
