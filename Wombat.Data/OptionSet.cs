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

        public bool CanDelete { get; set; } = true;

        public bool CanEdit { get; set; } = true;

        public static int kTextId = 1;
        public static int kEPAScaleId = 2;
    }
}
