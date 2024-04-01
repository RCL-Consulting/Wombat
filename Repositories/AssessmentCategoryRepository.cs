using Wombat.Contracts;
using Wombat.Data;

namespace Wombat.Repositories
{
    public class AssessmentCategoryRepository : GenericRepository<AssessmentCategory>, IAssessmentCategoryRepository
    {
        public AssessmentCategoryRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
