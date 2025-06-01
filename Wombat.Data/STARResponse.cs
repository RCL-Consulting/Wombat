using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Data
{
    public class STARResponse : BaseEntity
    {
        public int STARApplicationId { get; set; }

        [ForeignKey(nameof(STARApplicationId))]
        public STARApplication? STARApplication { get; set; }

        public int QuestionId { get; set; }

        [ForeignKey(nameof(QuestionId))]
        public STARItem? Question { get; set; }

        public int? OptionId { get; set; }

        [ForeignKey(nameof(OptionId))]
        public Option? Option { get; set; }

        public string Comment { get; set; } = "";
    }

}
