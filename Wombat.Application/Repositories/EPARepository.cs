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

using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Contracts;
using Wombat.Data;

namespace Wombat.Application.Repositories
{
    public class EPARepository : GenericRepository<EPA>, IEPARepository
    {
        private readonly IAssessmentFormRepository assessmentFormRepository;
        private readonly ISubSpecialityRepository subSpecialityRepository;

        public EPARepository( ApplicationDbContext context,
                              IAssessmentFormRepository assessmentFormRepository,
                              ISubSpecialityRepository subSpecialityRepository) : base(context)
        {
            this.assessmentFormRepository = assessmentFormRepository;
            this.subSpecialityRepository = subSpecialityRepository;
        }

        public override async Task<EPA?> GetAsync(int? id)
        {
            if (id == null)
            {
                return null;
            }

            var epa = await context.EPAs
                .Include(s => s.Forms)
                .ThenInclude(sc => sc.Form) 
                .Include(s => s.EPACurricula)
                .ThenInclude(sc => sc.EPAScaleOption)
                .AsTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (epa != null)
            {
                epa.SubSpeciality = await subSpecialityRepository.GetAsync(epa.SubSpecialityId);

                return epa;
            }

            return null;
        }

        public override async Task<List<EPA>?> GetAllAsync()
        {
            var EPAs = await context.EPAs
                .Include(s => s.Forms)
                .ThenInclude(sc => sc.Form)
                .AsTracking()
                .ToListAsync();

            if (EPAs != null)
            {
                foreach (var EPA in EPAs)
                {
                    EPA.SubSpeciality = await subSpecialityRepository.GetAsync(EPA.SubSpecialityId);
                }
            }
            return EPAs;
        }

        public async Task<List<EPA>?> GetEPAListBySubspeciality(int id)
        {
            var EPAs = await context.EPAs
                .Where(s => s.SubSpecialityId == id)
                .Include(e => e.EPACurricula)
                    .ThenInclude(c => c.EPAScaleOption)
                .Include(e => e.Forms)
                    .ThenInclude(sc => sc.Form)
                .AsTracking()
                .ToListAsync();

            if (EPAs != null)
            {
                foreach (var EPA in EPAs)
                {
                    EPA.SubSpeciality = await subSpecialityRepository.GetAsync(EPA.SubSpecialityId);
                }
            }

            return EPAs;
        }

        public async Task<List<AssessmentForm>?> GetFormsByEPA(int id)
        {
            var epa = await GetAsync(id);

            var forms = new List<AssessmentForm>();
            foreach (var form in epa.Forms)
            {
                forms.Add(form.Form);
            }

            return forms;
        }
    }
}
