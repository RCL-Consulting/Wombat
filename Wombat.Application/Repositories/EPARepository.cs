using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Contracts;
using Wombat.Data;

namespace Wombat.Application.Repositories
{
    public class EPARepository : GenericRepository<EPA>, IEPARepository
    {
        private readonly IAssessmentFormRepository assessmentFormRepository;
        private readonly ISubSpecialityRepository subSpecialityRepository;

        public EPARepository( ApplicationDbContext context,
                              IAssessmentFormRepository assessmentFormRepository,
                              ISubSpecialityRepository subSpecialityRepository) : base(context)
        {
            this.assessmentFormRepository = assessmentFormRepository;
            this.subSpecialityRepository = subSpecialityRepository;
        }

        public override async Task<EPA?> GetAsync(int? id)
        {
            if (id == null)
            {
                return null;
            }

            var epa = await context.EPAs
                .Include(s => s.Forms)
                .ThenInclude(sc => sc.Form) 
                .Include(s => s.EPACurricula)
                .ThenInclude(sc => sc.EPAScaleOption)
                .AsTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (epa != null)
            {
                epa.SubSpeciality = await subSpecialityRepository.GetAsync(epa.SubSpecialityId);

                return epa;
            }

            return null;
        }

        public override async Task<List<EPA>?> GetAllAsync()
        {
            var EPAs = await context.EPAs
                .Include(s => s.Forms)
                .ThenInclude(sc => sc.Form)
                .AsTracking()
                .ToListAsync();

            if (EPAs != null)
            {
                foreach (var EPA in EPAs)
                {
                    EPA.SubSpeciality = await subSpecialityRepository.GetAsync(EPA.SubSpecialityId);
                }
            }
            return EPAs;
        }

        public async Task<List<EPA>?> GetEPAListBySubspeciality(int id)
        {
            var EPAs = await context.EPAs
                 .Where(s => s.SubSpecialityId == id)
                 .Include(s => s.Forms)
                 .ThenInclude(sc => sc.Form)
                 .AsTracking()
                 .ToListAsync();

            if (EPAs != null)
            {
                foreach (var EPA in EPAs)
                {
                    EPA.SubSpeciality = await subSpecialityRepository.GetAsync(EPA.SubSpecialityId);
                }
            }
            return EPAs;
        }
    }
}
