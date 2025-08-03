using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wombat.Common.Constants;

namespace Wombat.Data
{
    public class AssessmentEvent : BaseEntity
    {
        public string ActorId { get; set; }
        [ForeignKey(nameof(ActorId))]
        public WombatUser? Actor { get; set; }

        public AssessmentEventType Type { get; set; }

        public int? AssessmentRequestId { get; set; }
        [ForeignKey(nameof(AssessmentRequestId))]
        public AssessmentRequest? AssessmentRequest { get; set; }

        public int? LoggedAssessmentId { get; set; }
        [ForeignKey(nameof(LoggedAssessmentId))]
        public LoggedAssessment? LoggedAssessment { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string? Message { get; set; }  // user-entered comment or system-generated description
    }


}
