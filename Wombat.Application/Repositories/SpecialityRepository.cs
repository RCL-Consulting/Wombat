using Microsoft.EntityFrameworkCore;
using Wombat.Application.Contracts;
using Wombat.Data;

namespace Wombat.Application.Repositories
{
    public class SpecialityRepository : GenericRepository<Speciality>, ISpecialityRepository
    {
        public SpecialityRepository(ApplicationDbContext context) : base(context)
        {

        }

        public override async Task<Speciality?> GetAsync(int? id)
        {
            if (id == null)
            {
                return null;
            }

            var speciality = await base.GetAsync(id);

            if (speciality != null)
            {
                var subSpecialities = context.Entry(speciality);

                subSpecialities.Collection(e => e.SubSpecialities)
                     .Query()
                     .Load();
                return speciality;
            }

            return null;
        }

        public override async Task<List<Speciality>?> GetAllAsync()
        {
            var specialities = await base.GetAllAsync();

            if (specialities != null)
            {
                foreach (var speciality in specialities)
                {
                    var subSpecialities = context.Entry(speciality);

                    subSpecialities.Collection(e => e.SubSpecialities)
                         .Query()
                         .Load();
                }
            }
            return specialities;
        }
    }
}
