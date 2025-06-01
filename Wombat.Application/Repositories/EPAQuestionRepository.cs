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
    public class EPAQuestionRepository : GenericRepository<STARItem>, IEPAQuestionRepository
    {
        public EPAQuestionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<STARItem>> GetByEPAIdAsync(int epaId)
        {
            return await context.STARItems
                .Where(q => q.EPAId == epaId)
                .Include(q => q.OptionsSet)
                .ThenInclude(os => os.Options)
                .OrderBy(q => q.Rank)
                .ToListAsync();
        }

        // Optionally override GetAllAsync for eager loading
        public override async Task<List<STARItem>?> GetAllAsync()
        {
            return await context.STARItems
                .Include(q => q.OptionsSet)
                .ThenInclude(os => os.Options)
                .Include(q => q.EPA)
                .OrderBy(q => q.EPAId)
                .ThenBy(q => q.Rank)
                .ToListAsync();
        }

        public async Task<List<EPA>> GetAllWithSpecialitiesAndQuestionCountsAsync()
        {
            var epas = await context.EPAs
                .Include(e => e.SubSpeciality)!
                    .ThenInclude(s => s.Speciality)
                .ToListAsync();

            var questionCounts = await context.STARItems
                .GroupBy(q => q.EPAId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);

            foreach (var epa in epas)
            {
                epa.QuestionCount = questionCounts.TryGetValue(epa.Id, out var count) ? count : 0; // requires EPA.QuestionCount property
            }

            return epas;
        }
    }

}
