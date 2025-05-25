using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Common.Models
{
    public class CoordinatorDashboardVM
    {
        public List<WombatUserVM> PendingTrainees { get; set; } = new();
    }
}
