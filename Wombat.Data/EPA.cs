using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wombat.Data
{
    public class EPA: BaseEntity
    {
        public EPA()
        {
            Name = "";
            Description = "";
            EPACurricula = new List<EPACurriculum>();
        }

        public string Description { get; set; }

        public string Name { get; set; }

        public int SubSpecialityId { get; set; }

        [ForeignKey("SubSpecialityId")]
        [DeleteBehavior(DeleteBehavior.Cascade)]
        public SubSpeciality? SubSpeciality { get; set; }

        public ICollection<EPAForm> Forms { get; set; }

        public List<EPACurriculum> EPACurricula { get; set; }
    }
}
