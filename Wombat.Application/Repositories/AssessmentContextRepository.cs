using Wombat.Application.Contracts;
using Wombat.Data;

namespace Wombat.Application.Repositories
{
    public class AssessmentContextRepository : GenericRepository<AssessmentContext>, IAssessmentContextRepository
    {
        private readonly IAssessmentTemplateRepository assessmentTemplateRepository;

        public AssessmentContextRepository( ApplicationDbContext context,
                                            IAssessmentTemplateRepository assessmentTemplateRepository) : base(context)
        {
            this.assessmentTemplateRepository=assessmentTemplateRepository;
        }


        public override async Task<AssessmentContext?> GetAsync(int? id)
        {
            if (id == null)
            {
                return null;
            }

            var assessmentContext = await base.GetAsync(id);

            if (assessmentContext!=null)
            {
                assessmentContext.AssessmentTemplate = await assessmentTemplateRepository.GetAsync(assessmentContext.AssessmentTemplateId);

                return assessmentContext;
            }

            return null;
        }
    }
}
