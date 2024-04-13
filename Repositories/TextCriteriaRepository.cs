using Wombat.Contracts;
using Wombat.Data;

namespace Wombat.Repositories
{
    public class TextCriteriaRepository : GenericRepository<TextCriterion>, ITextCriteriaRepository
    {
        public TextCriteriaRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
