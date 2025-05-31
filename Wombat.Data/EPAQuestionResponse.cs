using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Data
{
    public class EPAQuestionResponse : BaseEntity
    {
        public int? OptionId { get; set; }

        [ForeignKey("OptionId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public Option? Option { get; set; }

        public int QuestionId { get; set; }

        [ForeignKey("QuestionId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public EPAQuestion? Question { get; set; }

        public string TraineeId { get; set; }

        [ForeignKey("TraineeId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public WombatUser? Trainee { get; set; }

        public string Comment { get; set; } = "";

        public DateTime DateAnswered { get; set; } = DateTime.UtcNow;
    }

}
