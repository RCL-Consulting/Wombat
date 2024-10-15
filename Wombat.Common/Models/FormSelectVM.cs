using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Common.Models
{
    public class FormSelectVM
    {
        public int Id { get; set; }

        public int DisplayId { get; set; }
        static public int NextDisplayId { get; set; } = 1;
    }
}
