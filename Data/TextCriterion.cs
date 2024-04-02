namespace Wombat.Data
{
    public class TextCriterion: BaseEntity
    {
        public string Description { get; set; }
        public virtual ICollection<Category> Categories { get; set; }

        public TextCriterion()
        {
            this.Categories = new HashSet<Category>();
        }
    }
}
