using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Data
{
    public class STARApplication : BaseEntity
    {
        public string TraineeId { get; set; } = null!;

        [ForeignKey(nameof(TraineeId))]
        public WombatUser? Trainee { get; set; }

        public int EPAId { get; set; }

        [ForeignKey(nameof(EPAId))]
        public EPA? EPA { get; set; }

        public DateTime SubmittedOn { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedOn { get; set; }
        public DateTime? DeclinedOn { get; set; }

        public string? FreeTextReflection { get; set; }
        public string? Feedback { get; set; }

        public ICollection<STARResponse> Responses { get; set; } = new List<STARResponse>();
    }
}
