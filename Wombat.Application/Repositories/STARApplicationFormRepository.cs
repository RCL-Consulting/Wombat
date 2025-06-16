using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wombat.Application.Contracts;
using Wombat.Common.Models;
using Wombat.Data;

namespace Wombat.Application.Repositories
{
    public class STARApplicationFormRepository : GenericRepository<STARApplicationForm>, ISTARApplicationFormRepository
    {
        public STARApplicationFormRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<STARApplicationForm?> GetByEPAIdAsync(int epaId)
        {
            return await context.STARApplicationForms
                .Include(f => f.EPA)
                    .ThenInclude(e => e.SubSpeciality)
                        .ThenInclude(s => s.Speciality)
                .Include(f => f.STARItems)
                    .ThenInclude(i => i.OptionsSet)
                        .ThenInclude(os => os.Options)
                .Where(f => f.EPAId == epaId)
                .OrderByDescending(f => f.CreatedOn) // if multiple forms per EPA, pick latest
                .FirstOrDefaultAsync();
        }

        // Optionally override GetAllAsync for eager loading
        public override async Task<List<STARApplicationForm>?> GetAllAsync()
        {
            return await context.STARApplicationForms
                .Include(f => f.EPA)
                    .ThenInclude(e => e.SubSpeciality)
                        .ThenInclude(s => s.Speciality)
                .Include(f => f.STARItems)
                    .ThenInclude(i => i.OptionsSet)
                        .ThenInclude(os => os.Options)
                .OrderByDescending(f => f.CreatedOn)
                .ToListAsync();
        }

        public override async Task<STARApplicationForm?> GetAsync(int? id)
        {
            return await context.STARApplicationForms
                .Include(f => f.EPA)
                    .ThenInclude(e => e.SubSpeciality)
                        .ThenInclude(s => s.Speciality)
                .Include(f => f.STARItems) // if needed
                .FirstOrDefaultAsync(f => f.Id == id);
        }
    }

}
