using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Common.Models
{
    public class InstitutionVM
    {
        public InstitutionVM()
        {
            Name = "";
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public byte[]? Logo { get; set; }
    }

}
