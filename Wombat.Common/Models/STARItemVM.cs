using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Common.Models
{
    public class STARItemVM
    {
        public int? Id { get; set; }

        public int DisplayId { get; set; }
        public int Rank { get; set; }

        public string Heading { get; set; } = ""; // NEW

        public string Description { get; set; } = "";

        public int OptionSetId { get; set; }
        public OptionSetVM? OptionSet { get; set; }

        public bool IsRequired { get; set; }

        public List<SelectListItem> AvailableOptionSets { get; set; } = new();
    }
}
