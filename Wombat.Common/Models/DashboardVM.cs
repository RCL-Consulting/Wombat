using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Common.Models
{
    public class DashboardVM
    {
        public WombatUserVM User { get; set; }

        public List<EPAVM> EPAList { get; set; }
    }
}
