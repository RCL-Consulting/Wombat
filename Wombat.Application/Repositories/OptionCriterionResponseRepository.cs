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
using Wombat.Data;

namespace Wombat.Application.Repositories
{
    public class OptionCriterionResponseRepository : GenericRepository<OptionCriterionResponse>, IOptionCriterionResponseRepository
    {
        public OptionCriterionResponseRepository( ApplicationDbContext context ) : base(context)
        {
        }

        public async Task<List<OptionCriterionResponse>?> GetByAssessmentIdAsync(int assessmentID)
        {
            var responses = await context.OptionCriterionResponses
                .Where(x => x.AssessmentId == assessmentID)
                .Include( o => o.Option)
                .Include(s => s.Criterion)
                .ThenInclude(sc => sc.OptionsSet)
                .ToListAsync();

            //if (specialities != null)
            //{
            //    foreach (var speciality in specialities)
            //    {
            //        var subSpecialities = context.Entry(speciality);

            //        subSpecialities.Collection(e => e.SubSpecialities)
            //             .Query()
            //             .Load();
            //    }
            //}

            return responses;
        }
    }
}
