using System.ComponentModel.DataAnnotations;

namespace Wombat.Common.Models
{
    public class AssessmentTemplateVM
    {
        public AssessmentTemplateVM()
        {
            Name = "";
            OptionCriteria = new List<OptionCriterionVM>();
        }
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        public List<OptionCriterionVM> OptionCriteria { get; set; }

        public List<OptionSetVM>? OptionSets { get; set; }

        public EPAVM? EPAVM { get; set; }
    }
}
