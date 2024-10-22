using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Common.Models
{
    public class PortfolioVM
    {
        public WombatUserVM Trainee { get; set; }

        public List<EPAVM> EPAList { get; set; }

        public Dictionary<int, int> AssessmentsPerEPA { get; set; }

        public Dictionary<int, int> ScorePerEPA { get; set; }
    }
}
