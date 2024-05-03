using Wombat.Application.Contracts;
using Wombat.Data;

namespace Wombat.Application.Repositories
{
    public class TextCriteriaRepository : GenericRepository<TextCriterion>, ITextCriteriaRepository
    {
        public TextCriteriaRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
