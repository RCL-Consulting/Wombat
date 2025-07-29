using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using Wombat.Common.Constants;
using Wombat.Common.Models;
using Wombat.Data;
using static Wombat.Data.WombatUser;

namespace Wombat.Web.Controllers
{
    [Authorize]
    public class CoordinatorsController : Controller
    {
        private readonly UserManager<WombatUser> userManager;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IMapper mapper;

        public CoordinatorsController( UserManager<WombatUser> userManager,
                                       IHttpContextAccessor httpContextAccessor,
                                       IMapper mapper )
        {
            this.userManager = userManager;
            this.httpContextAccessor = httpContextAccessor;
            this.mapper = mapper;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Claims.ApproveTrainee)]
        public async Task<IActionResult> Approve(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            user.ApprovalStatus = eApprovalStatus.Approved;
            await userManager.UpdateAsync(user);
            await userManager.RemoveFromRoleAsync(user, "PendingTrainee");
            await userManager.AddToRoleAsync(user, "Trainee");

            return RedirectToAction("Pending");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Claims.ApproveTrainee)]
        public async Task<IActionResult> Reject(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            user.ApprovalStatus = eApprovalStatus.Rejected;
            await userManager.UpdateAsync(user);
            await userManager.RemoveFromRoleAsync(user, "PendingTrainee");

            return RedirectToAction("Pending");
        }

        public async Task<IActionResult> DashboardAsync()
        {
            return View();
        }

        public async Task<IActionResult> PendingAsync()
        {
            var userId = userManager.GetUserId(httpContextAccessor.HttpContext.User);
            var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext.User);
            var roles = await userManager.GetRolesAsync(user);

            var dashboard = new DashboardVM
            {
                User = mapper.Map<WombatUserVM>(user),
                EPAList = new List<EPAVM>()
            };

            // Step 1: Load all pending users (from DB)
            var potentialPendingUsers = await userManager.Users
                .Where(u => u.ApprovalStatus == eApprovalStatus.Pending &&
                            u.InstitutionId == user.InstitutionId &&
                            u.SubSpecialityId == user.SubSpecialityId)
                .ToListAsync();

            // Step 2: Filter in-memory by role
            var pendingUsers = new List<WombatUser>();
            foreach (var u in potentialPendingUsers)
            {
                if (await userManager.IsInRoleAsync(u, "PendingTrainee"))
                    pendingUsers.Add(u);
            }

            var pendingVMs = pendingUsers.Select(u => new WombatUserVM
            {
                Id = u.Id,
                Name = u.Name,
                Surname = u.Surname,
                Email = u.Email,
            }).ToList();

            dashboard.Coordinator = new CoordinatorDashboardVM
            {
                PendingTrainees = pendingVMs
            };

            return View(dashboard.Coordinator);
        }
    }
}
