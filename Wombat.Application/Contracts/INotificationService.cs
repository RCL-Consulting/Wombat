using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Application.Contracts
{
    public interface INotificationService
    {
        Task NotifyAsync(string userId, string subject, string message);
    }
}
