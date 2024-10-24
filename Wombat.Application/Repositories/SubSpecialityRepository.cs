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
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wombat.Application.Contracts;
using Wombat.Data;

namespace Wombat.Application.Repositories
{
    public class SubSpecialityRepository : GenericRepository<SubSpeciality>, ISubSpecialityRepository
    {
        private readonly ISpecialityRepository specialityRepository;

        public SubSpecialityRepository( ApplicationDbContext context,
                                        ISpecialityRepository specialityRepository) : base(context)
        {
            this.specialityRepository = specialityRepository;
        }

        public override async Task<SubSpeciality?> GetAsync(int? id)
        {
            if (id == null)
            {
                return null;
            }

            var subspeciality = await base.GetAsync(id);

            if (subspeciality != null)
            {
                subspeciality.Speciality = await specialityRepository.GetAsync(subspeciality.SpecialityId);

                return subspeciality;
            }

            return null;
        }

        public override async Task<List<SubSpeciality>?> GetAllAsync()
        {
            var subspecialities = await base.GetAllAsync();

            if (subspecialities != null)
            {
                foreach (var subspeciality in subspecialities)
                {
                    subspeciality.Speciality = await specialityRepository.GetAsync(subspeciality.SpecialityId);
                }
            }
            return subspecialities;
        }

        public async Task<List<SubSpeciality>?> GetSubSpecialitiesBySpecialityAsync(int id)
        {
            var subSpecialites = await context.SubSpecialities
                .Where(x => x.SpecialityId == id)
                .ToListAsync();

            if (subSpecialites != null)
            {
                foreach (var assessment in subSpecialites)
                {
                    //assessment.Trainee = await userManager.FindByIdAsync(assessment.TraineeId);
                    //assessment.Assessor = await userManager.FindByIdAsync(assessment.AssessorId);
                    //assessment.EPA = await EPARepository.GetAsync(assessment.EPAId);
                }
            }
            return subSpecialites;
        }
    }
}
