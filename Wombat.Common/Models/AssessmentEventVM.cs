using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wombat.Common.Constants;

namespace Wombat.Common.Models
{
    public class AssessmentEventVM
    {
        public int Id { get; set; }
        public AssessmentEventType Type { get; set; }
        public string? Message { get; set; }
        public DateTime Timestamp { get; set; }

        public string ActorId { get; set; }
        public WombatUserVM? Actor { get; set; }
    }

}
