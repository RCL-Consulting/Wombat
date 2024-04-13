using System.ComponentModel.DataAnnotations;

namespace Wombat.Models
{
    public class EnumCriterionVM 
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }
    }
}
