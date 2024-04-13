using Wombat.Contracts;
using Wombat.Data;

namespace Wombat.Repositories
{
    public class EnumCriteriaRepository : GenericRepository<EnumCriterion>, IEnumCriteriaRepository
    {
        public EnumCriteriaRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
