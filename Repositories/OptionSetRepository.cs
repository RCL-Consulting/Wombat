using Microsoft.EntityFrameworkCore;
using Wombat.Contracts;
using Wombat.Data;

namespace Wombat.Repositories
{
    public class OptionSetRepository : GenericRepository<OptionSet>, IOptionSetRepository
    {
        public OptionSetRepository(ApplicationDbContext context) : base(context)
        {

        }

        public override async Task<OptionSet?> GetAsync(int? id)
        {
            if (id == null)
            {
                return null;
            }

            var optionSet = await base.GetAsync(id);

            if (optionSet!=null)
            {
                var Options = context.Entry(optionSet);

                Options.Collection(e => e.Options)
                     .Query()
                     .OrderBy(c => c.Rank)
                     .Load();
                return optionSet;
            }

            return null;
        }
    }
}
