using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Common.Models
{
    public class EPACurriculumVM : Collection
    {

        [Required(ErrorMessage = "Number of months is required")]
        [Display(Name = "Number of months")]
        public int NumberOfMonths { get; set; }

        public int EPAId { get; set; }

        public EPAVM? EPA { get; set; }

        public int EPAScaleId { get; set; }

        public OptionVM? EPAScaleOption { get; set; }

        public static List<SelectListItem> AvailableScaleOptions { get; set; }
}
}
