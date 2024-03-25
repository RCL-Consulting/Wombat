using System.ComponentModel.DataAnnotations;

namespace Wombat.Models
{
    public class AssessmentCategoryVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")] 
        public string Name { get; set; }
    }
}
