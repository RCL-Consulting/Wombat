namespace Wombat.Data
{
    public class EnumCriterion: BaseEntity
    {
        public string Description { get; set; }
        public ICollection<EnumOption> EnumOptions { get; set; }
        public virtual ICollection<Category> Categories { get; set; }

        public EnumCriterion()
        {
            this.Categories = new HashSet<Category>();
        }
    }
}
