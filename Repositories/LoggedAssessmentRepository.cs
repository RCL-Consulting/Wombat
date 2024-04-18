using Wombat.Contracts;
using Wombat.Data;

namespace Wombat.Repositories
{
    public class LoggedAssessmentRepository : GenericRepository<LoggedAssessment>, ILoggedAssessmentRepository
    {
        private readonly IOptionSetRepository optionSetRepository;

        public LoggedAssessmentRepository(ApplicationDbContext context ) : base(context)
        {
        }
    }
}
