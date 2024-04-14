namespace Wombat.Models
{
    public class OptionVM
    {
        public OptionVM()
        {
            Description = "";
        }
        public int Id { get; set; }
        public string Description { get; set; }
        public int Rank { get; set; }
    }
}
