using System.ComponentModel.DataAnnotations;

namespace Wombat.Common.Models
{
    public class AssessmentTemplateVM: Collection
    {
        public AssessmentTemplateVM()
        {
            Name = "";
            OptionCriteria = new List<OptionCriterionVM>();
        }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        public List<OptionCriterionVM> OptionCriteria { get; set; }

        public EPAVM? EPAVM { get; set; }

        public static List<AssessmentTemplateVM> Templates = new List<AssessmentTemplateVM>();
    }
}
