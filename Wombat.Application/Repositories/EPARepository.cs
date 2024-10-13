using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Contracts;
using Wombat.Data;

namespace Wombat.Application.Repositories
{
    public class EPARepository : GenericRepository<EPA>, IEPARepository
    {
        private readonly IAssessmentTemplateRepository assessmentTemplateRepository;
        private readonly ISubSpecialityRepository subSpecialityRepository;

        public EPARepository( ApplicationDbContext context,
                              IAssessmentTemplateRepository assessmentTemplateRepository,
                              ISubSpecialityRepository subSpecialityRepository) : base(context)
        {
            this.assessmentTemplateRepository = assessmentTemplateRepository;
            this.subSpecialityRepository = subSpecialityRepository;
        }


        public override async Task<EPA?> GetAsync(int? id)
        {
            if (id == null)
            {
                return null;
            }

            var EPA = await base.GetAsync(id);

            if (EPA!=null)
            {
                var Templates = context.Entry(EPA);

                Templates.Collection(e => e.Templates)
                     .Query()
                     .Load();

                EPA.SubSpeciality = await context.SubSpecialities.FindAsync(EPA.SubSpecialityId);

                return EPA;
                //EPA.AssessmentTemplate = await assessmentTemplateRepository.GetAsync(EPA.AssessmentTemplateId);

                //return EPA;
            }

            return null;
        }

        public override async Task<List<EPA>?> GetAllAsync()
        {
            var EPAs = await base.GetAllAsync();

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
