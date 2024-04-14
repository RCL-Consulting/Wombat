namespace Wombat.Data
{
    public class OptionSet : BaseEntity
    {
        public OptionSet()
        {
            Description = "";
            DisplayRank = true;
            Options = new List<Option>();
        }

        public string Description { get; set; }

        public bool DisplayRank { get; set; }

        public List<Option> Options { get; set; }
    }
}
