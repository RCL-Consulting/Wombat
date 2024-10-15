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
                .ThenInclude(sc => sc.Form) // Load associated Courses
                .FirstOrDefaultAsync(s => s.Id == id);

            if (epa != null)
            {
                var Forms = context.Entry(epa);

                Forms.Collection(e => e.Forms)
                     .Query()
                     .Load();

                epa.SubSpeciality = await subSpecialityRepository.GetAsync(epa.SubSpecialityId);

                return epa;
                //EPA.AssessmentForm = await assessmentFormRepository.GetAsync(EPA.AssessmentFormId);

                //return EPA;
            }

            return null;
        }

        public override async Task<List<EPA>?> GetAllAsync()
        {
            var EPAs = await context.EPAs
                .Include(s => s.Forms)
                .ThenInclude(sc => sc.Form)
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
