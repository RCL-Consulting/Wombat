﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wombat.Data;

namespace Wombat.Application.Contracts
{
    public interface IRegistrationInvitationRepository : IGenericRepository<RegistrationInvitation>
    {
        Task<RegistrationInvitation?> GetByTokenAsync(string token);
        Task MarkAsUsedAsync(string token);
    }
}
