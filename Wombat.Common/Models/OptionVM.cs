namespace Wombat.Common.Models
{
    public class OptionVM : Collection
    {
        public OptionVM()
        {
            Description = "";
        }
        public string Description { get; set; }
        public int Rank { get; set; }
    }
}
