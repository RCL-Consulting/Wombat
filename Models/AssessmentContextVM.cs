using System.ComponentModel.DataAnnotations;
using Wombat.Data;

namespace Wombat.Models
{
    public class AssessmentContextVM
    {
        public AssessmentContextVM()
        {
            Description = "";
        }
        public int Id { get; set; }
        public string Description { get; set; }

        public AssessmentCategory? AssessmentCategory { get; set; }
        public int AssessmentCategoryId { get; set; }

        public List<AssessmentCategoryVM>? Categories { get; set; }

    }
}
