using System.ComponentModel.DataAnnotations;

namespace Wombat.Common.Models
{
    public class OptionSetVM
    {
        public OptionSetVM()
        {
            Description = "";
            Options = new List<OptionVM>();
        }
        public int Id { get; set; }
        public string Description { get; set; }

        [Display(Name = "Display Rank")]
        public bool DisplayRank { get; set; }

        public List<OptionVM>? Options { get; set; }

        public bool CanDelete { get; set; } = true;

        public bool CanEdit { get; set; } = true;

    }
}
