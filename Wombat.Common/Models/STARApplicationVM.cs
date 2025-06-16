using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Common.Models
{
    public class STARApplicationVM
    {
        public int EPAId { get; set; }

        public string EPAName { get; set; } = "";

        public string SpecialityName { get; set; } = "";
        public int? SubSpecialityId { get; set; } // null if no sub-speciality
        public string SubSpecialityName { get; set; } = "";

        public List<STARItemVM> Items { get; set; } = new();

    }
}
