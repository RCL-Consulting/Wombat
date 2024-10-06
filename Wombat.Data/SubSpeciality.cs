using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Data
{
    public class SubSpeciality : BaseEntity
    {
        public SubSpeciality()
        {
            Name = "";
        }
        public string Name { get; set; }

        public int SpecialityId { get; set; }

        [ForeignKey("SpecialityId")]
        [DeleteBehavior(DeleteBehavior.Cascade)]
        public Speciality? Speciality { get; set; }

    }
}
