using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Wombat.Common.Models.STARApplicationVM;

namespace Wombat.Common.Models
{
    public class STARApplicationFormVM
    {
        public int Id { get; set; }

        [Display(Name = "Form Name")]
        public string Name { get; set; } = "";

        [Display(Name = "Created On")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [Display(Name = "Published")]
        public bool IsPublished { get; set; }

        [Display(Name = "EPA")]
        public int EPAId { get; set; }

        public int SpecialityId { get; set; }
        public int SubSpecialityId { get; set; }

        public string EPAName { get; set; } = "";
        public string SpecialityName { get; set; } = "";
        public string SubSpecialityName { get; set; } = "";

        public List<STARItemVM?> STARItems { get; set; } = new();
    }
}
