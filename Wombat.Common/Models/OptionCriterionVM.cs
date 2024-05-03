using System.ComponentModel.DataAnnotations;

namespace Wombat.Common.Models
{
    public class OptionCriterionVM 
    {
        public OptionCriterionVM()
        {
            Description = "";
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        public int OptionSetId { get; set; }
        public OptionSetVM? OptionsSet { get; set; }

        public int Rank { get; set; }

        
    }
}
