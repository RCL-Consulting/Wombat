using System.ComponentModel.DataAnnotations;

namespace Wombat.Common.Models
{
    public class AssessmentFormVM
    {
        public AssessmentFormVM()
        {
            Name = "";
            OptionCriteria = new List<OptionCriterionVM>();
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        public List<OptionCriterionVM> OptionCriteria { get; set; }

        public EPAVM? EPAVM { get; set; }

        public bool CanDelete { get; set; } = true;
    }
}
