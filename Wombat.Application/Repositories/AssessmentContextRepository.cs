using Wombat.Application.Contracts;
using Wombat.Data;

namespace Wombat.Application.Repositories
{
    public class AssessmentContextRepository : GenericRepository<AssessmentContext>, IAssessmentContextRepository
    {
        private readonly IAssessmentCategoryRepository assessmentCategoryRepository;

        public AssessmentContextRepository( ApplicationDbContext context,
                                            IAssessmentCategoryRepository assessmentCategoryRepository) : base(context)
        {
            this.assessmentCategoryRepository=assessmentCategoryRepository;
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
                assessmentContext.AssessmentCategory = await assessmentCategoryRepository.GetAsync(assessmentContext.AssessmentCategoryId);

                return assessmentContext;
            }

            return null;
        }
    }
}
