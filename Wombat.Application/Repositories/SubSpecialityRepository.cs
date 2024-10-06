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
    }
}
