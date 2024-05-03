using Wombat.Application.Contracts;
using Wombat.Data;

namespace Wombat.Application.Repositories
{
    public class OptionCriteriaRepository : GenericRepository<OptionCriterion>, IEnumCriteriaRepository
    {
        private readonly IOptionSetRepository optionSetRepository;

        public OptionCriteriaRepository( ApplicationDbContext context,
                                         IOptionSetRepository optionSetRepository) : base(context)
        {
            this.optionSetRepository=optionSetRepository;
        }

        public override async Task<OptionCriterion?> GetAsync(int? id)
        {
            if (id == null)
            {
                return null;
            }

            var criterion = await base.GetAsync(id);

            if (criterion!=null)
            {
                criterion.OptionsSet = await optionSetRepository.GetAsync(criterion.OptionSetId);
                return criterion;
            }

            return null;
        }
    }
}
