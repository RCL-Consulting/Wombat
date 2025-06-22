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
    public class AssessmentFormRepository : GenericRepository<AssessmentForm>, IAssessmentFormRepository
    {
        private readonly IOptionSetRepository optionSetRepository;

        public AssessmentFormRepository( ApplicationDbContext context,
                                         IOptionSetRepository optionSetRepository ) : base(context)
        {
            this.optionSetRepository=optionSetRepository;
        }

        public async Task<IEnumerable<AssessmentForm>> GetScopedFormsAsync( WombatUser user,
                                                                            IList<string> roles )
        {
            var isGlobalAdmin = roles.Contains(Role.Administrator.ToStringValue());

            var allForms = await GetAllAsync(); // or optimize with .AsQueryable() if needed

            if (isGlobalAdmin)
                return allForms;

            return allForms.Where(f =>
                f.InstitutionId == null || // system-wide forms always visible
                (roles.Contains(Role.InstitutionalAdmin.ToStringValue()) &&
                    (user.InstitutionId == null || f.InstitutionId == user.InstitutionId)) ||
                (roles.Contains(Role.SpecialityAdmin.ToStringValue()) &&
                    f.SpecialityId == user.SubSpeciality?.SpecialityId &&
                    (user.InstitutionId == null || f.InstitutionId == user.InstitutionId)) ||
                (roles.Contains(Role.SubSpecialityAdmin.ToStringValue()) &&
                    f.SubSpecialityId == user.SubSpecialityId &&
                    (user.InstitutionId == null || f.InstitutionId == user.InstitutionId))
            );
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
                var entry = context.Entry(assessmentForm);

                // Load and order OptionCriteria
                await entry.Collection(e => e.OptionCriteria)
                    .Query()
                    .OrderBy(c => c.Rank)
                    .LoadAsync();

                // Load referenced OptionSets
                foreach (var optionCriterion in assessmentForm.OptionCriteria)
                {
                    optionCriterion.OptionsSet = await optionSetRepository.GetAsync(optionCriterion.OptionSetId);
                }

                // Load related EPAs through the EPAForm join table
                await entry.Collection(e => e.EPAs)
                    .Query()
                    .Include(epaForm => epaForm.EPA) // Make sure EPAForm has a navigation property to EPA
                    .LoadAsync();

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
