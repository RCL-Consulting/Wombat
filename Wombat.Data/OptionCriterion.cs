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

        public int AssessmentFormId { get; set; }

        [ForeignKey("AssessmentFormId")]
        [DeleteBehavior(DeleteBehavior.Cascade)]
        public AssessmentForm? AssessmentForm { get; set; }

        public int Rank { get; set; }
    }
}
