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
    public class RegistrationInvitationRepository : GenericRepository<RegistrationInvitation>, IRegistrationInvitationRepository
    {
        public RegistrationInvitationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<RegistrationInvitation?> GetByTokenAsync(string token)
        {
            return await context.RegistrationInvitations
               .Include(r => r.Speciality)
               .Include(r => r.SubSpeciality)
               .FirstOrDefaultAsync(r => r.Token == token && !r.IsUsed && r.ExpiryDate > DateTime.UtcNow);
        }

        public async Task MarkAsUsedAsync(string token)
        {
            var invitation = await context.RegistrationInvitations.FirstOrDefaultAsync(i => i.Token == token);
            if (invitation != null)
            {
                invitation.IsUsed = true;
                invitation.DateModified = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
        }
        public override async Task<List<RegistrationInvitation>> GetAllAsync()
        {
            return await context.RegistrationInvitations
                .Include(r => r.Institution)
                .Include(r => r.Speciality)
                .Include(r => r.SubSpeciality)
                .ToListAsync();
        }
    }
}
