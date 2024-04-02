namespace Wombat.Data
{
    public class Category: BaseEntity
    {
        public string Name { get; set; }
        public virtual ICollection<EnumCriterion> EnumCriteria { get; set; }
        public virtual ICollection<TextCriterion> TextCriteria { get; set; }

        public Category()
        {
            this.EnumCriteria = new HashSet<EnumCriterion>();
            this.TextCriteria = new HashSet<TextCriterion>();
        }
    }
}
