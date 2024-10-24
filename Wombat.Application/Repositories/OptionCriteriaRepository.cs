/*Copyright (C) 2024 RCL Consulting
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>
 */

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
