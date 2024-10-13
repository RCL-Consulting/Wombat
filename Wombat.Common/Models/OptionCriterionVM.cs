using System.ComponentModel.DataAnnotations;

namespace Wombat.Common.Models
{
    public class OptionCriterionVM :Collection
    {
        public OptionCriterionVM()
        {
            Description = "";
        }


        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        public int OptionSetId { get; set; }
        public OptionSetVM? OptionsSet { get; set; }

        public int Rank { get; set; }

        public static List<OptionSetVM> OptionsSets = new List<OptionSetVM>();
    }
}
