using System.ComponentModel.DataAnnotations;

namespace Wombat.Common.Models
{
    public class OptionSetVM
    {
        public OptionSetVM()
        {
            Description = "";
        }
        public int Id { get; set; }
        public string Description { get; set; }

        [Display(Name = "Display Rank")]
        public bool DisplayRank { get; set; }

        public List<OptionVM>? Options { get; set; }

    }
}
