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
            // One base query that always brings navs
            var q = context.OptionSets
                .Include(e => e.Institution)
                .Include(e => e.Speciality)
                .Include(e => e.SubSpeciality)
                .Include(e => e.Options); // load collection; we'll sort after

            List<OptionSet> list;

            // Global admin: see everything (with navs)
            if (roles.Contains(Role.Administrator.ToStringValue()))
            {
                list = await q.ToListAsync();
            }
            else
            {
                var ui = user.InstitutionId;
                var us = user.SpecialityId;
                var uss = user.SubSpecialityId;

                list = await q.Where(e =>
                       e.InstitutionId == null
                    || e.InstitutionId == ui
                    || (e.InstitutionId == ui && e.SpecialityId == us)
                    || (e.InstitutionId == ui && e.SpecialityId == us && e.SubSpecialityId == uss)
                ).ToListAsync();
            }

            // Ensure Options are ordered (Include doesn't guarantee order on the collection)
            foreach (var os in list)
                os.Options = os.Options.OrderBy(o => o.Rank).ToList();

            return list;
        }


        public override async Task<OptionSet?> GetAsync(int? id)
        {
            if (id is null) return null;

            var optionSet = await context.OptionSets
                .Include(e => e.Institution)
                .Include(e => e.Speciality)
                .Include(e => e.SubSpeciality)
                .FirstOrDefaultAsync(e => e.Id == id.Value);

            if (optionSet is null) return null;

            await context.Entry(optionSet)
                .Collection(e => e.Options)
                .Query()
                .OrderBy(o => o.Rank)
                .LoadAsync();

            return optionSet;
        }
    }
}
