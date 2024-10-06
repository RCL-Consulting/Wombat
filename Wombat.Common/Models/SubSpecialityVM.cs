using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Common.Models
{
    public class SubSpecialityVM
    {
        public SubSpecialityVM()
        {
            Name = "";
        }
        public int Id { get; set; }
        public string Name { get; set; }

        public int? SpecialityId { get; set; }
        public SpecialityVM? Speciality { get; set; }
    }
}
