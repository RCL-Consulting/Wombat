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

            if (roles.Contains(Role.Administrator.ToStringValue()))
                return await query.ToListAsync();

            if (roles.Contains(Role.InstitutionalAdmin.ToStringValue()) && user.InstitutionId != null)
            {
                return await query
                    .Where(f => f.InstitutionId == user.InstitutionId || f.InstitutionId == null)
                    .ToListAsync();
            }

            if (roles.Contains(Role.SpecialityAdmin.ToStringValue()) && user.SubSpeciality?.SpecialityId != null)
            {
                return await query
                    .Where(f =>
                        f.SpecialityId == user.SubSpeciality.SpecialityId &&
                        (f.InstitutionId == null || f.InstitutionId == user.InstitutionId))
                    .ToListAsync();
            }

            if (roles.Contains(Role.SubSpecialityAdmin.ToStringValue()) && user.SubSpecialityId != null)
            {
                return await query
                    .Where(f =>
                        f.SubSpecialityId == user.SubSpecialityId &&
                        (f.InstitutionId == null || f.InstitutionId == user.InstitutionId))
                    .ToListAsync();
            }

            return new List<OptionSet>();
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
