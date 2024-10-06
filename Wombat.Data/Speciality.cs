using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Data
{
    public class Speciality : BaseEntity
    {
        public Speciality()
        {
            Name = "";
            SubSpecialities = new List<SubSpeciality>();
        }

        public string Name { get; set; }
        public List<SubSpeciality> SubSpecialities { get; set; }
    }
}
