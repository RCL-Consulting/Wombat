using System.ComponentModel.DataAnnotations;

namespace Wombat.Common.Models
{
    public class TextCriterionVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

    }
}
