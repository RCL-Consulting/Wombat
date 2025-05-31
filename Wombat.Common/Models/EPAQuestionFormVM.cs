using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Common.Models
{
    public class EPAQuestionFormVM
    {
        public int? Id { get; set; }
        public int EPAId { get; set; }

        public string EPAName { get; set; } = "";

        public string SpecialityName { get; set; } = "";
        public int? SubSpecialityId { get; set; } // null if no sub-speciality
        public string SubSpecialityName { get; set; } = "";

        public List<EPAQuestionVM> Questions { get; set; } = new();

        public class EPAQuestionVM
        {
            public int? Id { get; set; } // for persisted EPAQuestions

            public int DisplayId { get; set; }

            public string Heading { get; set; } = ""; // NEW

            public string Description { get; set; } = "";

            public int OptionSetId { get; set; }

            public bool IsRequired { get; set; }

            public List<SelectListItem> AvailableOptionSets { get; set; } = new();
        }
    }
}
