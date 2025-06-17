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
    public class AssessmentFormRepository : GenericRepository<AssessmentForm>, IAssessmentFormRepository
    {
        private readonly IOptionSetRepository optionSetRepository;

        public AssessmentFormRepository( ApplicationDbContext context,
                                         IOptionSetRepository optionSetRepository ) : base(context)
        {
            this.optionSetRepository=optionSetRepository;
        }

        public override async Task<AssessmentForm?> GetAsync(int? id)
        {
            if (id == null)
            {
                return null;
            }

            var assessmentForm = await base.GetAsync(id);

            if (assessmentForm != null)
            {
                var OptionCriteria = context.Entry(assessmentForm);

                OptionCriteria.Collection(e => e.OptionCriteria)
                     .Query()
                     .OrderBy(c => c.Rank)
                     .Load();

                foreach (var optionCriterion in assessmentForm.OptionCriteria)
                {
                    optionCriterion.OptionsSet = await optionSetRepository.GetAsync(optionCriterion.OptionSetId);
                }

                return assessmentForm;
            }

            return null;
        }

        public async Task<List<AssessmentForm>?> GetScopedFormsForEPA(int epaId, int institutionId, int specialityId, int subSpecialityId)
        {
            return await context.EPAForms
                .Where(e => e.EPAId == epaId && e.Form != null)
                .Select(e => e.Form!)
                .Where(f =>
                    (f.SubSpecialityId == subSpecialityId) ||
                    (f.SpecialityId == specialityId && f.SubSpecialityId == null) ||
                    (f.InstitutionId == institutionId && f.SpecialityId == null && f.SubSpecialityId == null) ||
                    (f.InstitutionId == null && f.SpecialityId == null && f.SubSpecialityId == null)
                )
                .Distinct()
                .ToListAsync();
        }
    }
}
