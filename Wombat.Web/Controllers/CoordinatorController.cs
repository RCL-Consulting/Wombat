using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static Wombat.Data.WombatUser;
using Wombat.Data;
using Microsoft.AspNetCore.Authorization;

namespace Wombat.Web.Controllers
{
    [Authorize(Roles = "Coordinator")]
    public class CoordinatorController : Controller
    {
        private readonly UserManager<WombatUser> _userManager;

        public CoordinatorController(UserManager<WombatUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Approve(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            user.ApprovalStatus = eApprovalStatus.Approved;
            await _userManager.UpdateAsync(user);
            await _userManager.RemoveFromRoleAsync(user, "PendingTrainee");
            await _userManager.AddToRoleAsync(user, "Trainee");

            return RedirectToAction("Index", "Home"); // or CoordinatorController's own dashboard
        }

        [HttpPost]
        public async Task<IActionResult> Reject(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            user.ApprovalStatus = eApprovalStatus.Rejected;
            await _userManager.UpdateAsync(user);
            await _userManager.RemoveFromRoleAsync(user, "PendingTrainee");

            return RedirectToAction("Index", "Home");
        }
    }
}
