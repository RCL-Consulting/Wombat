using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Common.Models
{
    public class SubSpecialitySelectVM
    {
        public SubSpecialitySelectVM()
        {
            Name = "";
        }
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
