// Wombat.Common.Models (or Wombat.Common.Domain)
using System;
using Wombat.Data;                  // for AssessmentRequest
using Wombat.Common.Constants;         // if you keep the enum here

namespace Wombat.Common.Models
{
    public static class AssessmentRequestExtensions
    {
        /// <summary>
        /// Returns the display/status for UI derived from the persisted base Status + dates.
        /// Only NotConducted/Expired are derived. Everything else mirrors the stored Status.
        /// </summary>
        public static AssessmentRequestStatus GetDisplayStatus(this AssessmentRequest r, DateTime nowUtc)
        {
            if (r == null) throw new ArgumentNullException(nameof(r));

            // Derive NotConducted if accepted, the scheduled time has passed, and not completed.
            if (r.Status == AssessmentRequestStatus.Accepted &&
                r.AssessmentDate.HasValue &&
                r.AssessmentDate.Value.ToUniversalTime() < nowUtc &&
                r.CompletionDate == null)
            {
                return AssessmentRequestStatus.NotConducted;
            }

            // Derive Expired if still requested, the scheduled time has passed, and not accepted.
            if (r.Status == AssessmentRequestStatus.Requested &&
                r.AssessmentDate.HasValue &&
                r.AssessmentDate.Value.ToUniversalTime() < nowUtc)
            {
                return AssessmentRequestStatus.Expired;
            }

            // Otherwise, just return the persisted base status.
            return r.Status;
        }
    }
}
