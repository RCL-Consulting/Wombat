using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Common.Models
{
    public class SpecialitySelectVM
    {
        public SpecialitySelectVM()
        {
            Name = "";
            SubSpecialities = new List<SubSpecialitySelectVM>();
        }
        public int Id { get; set; }

        public string Name { get; set; }

        [DisplayName("Subspecialities")]
        public List<SubSpecialitySelectVM>? SubSpecialities { get; set; }
    }

}
