using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wombat.Application.Contracts;
using Wombat.Data;

namespace Wombat.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IEmailSender _emailSender;
        private readonly UserManager<WombatUser> _userManager;

        public NotificationService(IEmailSender emailSender, UserManager<WombatUser> userManager)
        {
            _emailSender = emailSender;
            _userManager = userManager;
        }

        public async Task NotifyAsync(string userId, string subject, string message)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || string.IsNullOrWhiteSpace(user.Email))
                return;

            await _emailSender.SendEmailAsync(user.Email, subject, message);
        }
    }
}
