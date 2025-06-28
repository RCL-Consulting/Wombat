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

using Microsoft.EntityFrameworkCore;
using Wombat.Application.Contracts;
using Wombat.Common.Constants;
using Wombat.Data;

namespace Wombat.Application.Repositories
{
    public class OptionSetRepository : GenericRepository<OptionSet>, IOptionSetRepository
    {
        public OptionSetRepository(ApplicationDbContext context) : base(context)
        {

        }
        public override async Task<List<OptionSet>?> GetAllAsync()
        {
            return await context.OptionSets
                .Include(o => o.Options)
                .ToListAsync();
        }

        public async Task<List<OptionSet>> GetScopedOptionSetsAsync(WombatUser user, IList<string> roles)
        {
            var query = context.OptionSets.AsQueryable();

            // Global admin: see everything
            if (roles.Contains(Role.Administrator.ToStringValue()))
                return await query.ToListAsync();

            var userInstitutionId = user.InstitutionId;
            var userSpecialityId = user.SpecialityId;
            var userSubSpecialityId = user.SubSpecialityId;

            return await query.Where(optionSet =>
                // Global (no institution)
                optionSet.InstitutionId == null ||

                // Institution match
                optionSet.InstitutionId == userInstitutionId ||

                // Speciality match within institution
                (optionSet.InstitutionId == userInstitutionId &&
                 optionSet.SpecialityId == userSpecialityId) ||

                // Subspeciality match within speciality and institution
                (optionSet.InstitutionId == userInstitutionId &&
                 optionSet.SpecialityId == userSpecialityId &&
                 optionSet.SubSpecialityId == userSubSpecialityId)
            ).ToListAsync();
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
