using Wombat.Contracts;
using Wombat.Data;

namespace Wombat.Repositories
{
    public class AssessmentCategoryRepository : GenericRepository<Category>, IAssessmentCategoryRepository
    {
        public AssessmentCategoryRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
