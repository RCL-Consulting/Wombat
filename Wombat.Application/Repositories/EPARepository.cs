using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Contracts;
using Wombat.Data;

namespace Wombat.Application.Repositories
{
    public class EPARepository : GenericRepository<EPA>, IEPARepository
    {
        private readonly IAssessmentTemplateRepository assessmentTemplateRepository;

        public EPARepository( ApplicationDbContext context,
                              IAssessmentTemplateRepository assessmentTemplateRepository) : base(context)
        {
            this.assessmentTemplateRepository=assessmentTemplateRepository;
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
                return EPA;
                //EPA.AssessmentTemplate = await assessmentTemplateRepository.GetAsync(EPA.AssessmentTemplateId);

                //return EPA;
            }

            return null;
        }
    }
}
