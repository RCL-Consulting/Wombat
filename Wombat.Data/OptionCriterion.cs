using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wombat.Data
{
    public class OptionCriterion: BaseEntity
    {
        public OptionCriterion()
        {
            Description = "";
        }
        public string Description { get; set; }

        public int OptionSetId { get; set; }

        [ForeignKey("OptionSetId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public OptionSet? OptionsSet { get; set; }

        public int AssessmentTemplateId { get; set; }

        [ForeignKey("AssessmentTemplateId")]
        [DeleteBehavior(DeleteBehavior.Cascade)]
        public AssessmentTemplate? AssessmentTemplate { get; set; }

        public int Rank { get; set; }
    }
}
