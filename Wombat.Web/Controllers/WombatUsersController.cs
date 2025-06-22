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

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Wombat.Application.Contracts;
using Wombat.Application.Repositories;
using Wombat.Common.Constants;
using Wombat.Common.Models;
using Wombat.Data;

namespace Wombat.Controllers
{
    [Authorize]
    public class WombatUsersController : Controller
    {
        private readonly UserManager<WombatUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly ILogger<WombatUsersController> logger;
        private readonly IInstitutionRepository institutionRepository;
        private readonly ISpecialityRepository specialityRepository;
        private readonly ISubSpecialityRepository subSpecialityRepository;
        private readonly IUserContextService userContext;
        private readonly IMapper mapper;

        public WombatUsersController( UserManager<WombatUser> userManager,
                                      RoleManager<IdentityRole> roleManager,
                                      ILogger<WombatUsersController> logger,
                                      IInstitutionRepository institutionRepository,
                                      ISpecialityRepository specialityRepository,
                                      ISubSpecialityRepository subSpecialityRepository,
                                      IUserContextService userContext,
                                      IMapper mapper)
        {
            this.userManager=userManager;
            this.roleManager=roleManager;
            this.logger=logger;
            this.institutionRepository = institutionRepository;
            this.specialityRepository = specialityRepository;
            this.subSpecialityRepository = subSpecialityRepository;
            this.userContext = userContext;
            this.mapper=mapper;
        }

        public async Task<WombatUserVM> GetVMWithRoles(WombatUser user)
        {
            var allRoles = await roleManager.Roles.ToListAsync();
            var userRoles = await userManager.GetRolesAsync(user);

            var currentUser = await userManager.GetUserAsync(User);
            var currentUserRoles = await userManager.GetRolesAsync(currentUser);

            var allowedRoles = RoleHelper.DisplayRoles
                .Where(r => currentUserRoles.Any(cur => RoleHierarchy.CanAssign(cur, r.ToStringValue())))
                .Select(r => r.ToStringValue())
                .ToHashSet();
            var vm = mapper.Map<WombatUserVM>(user);

            var sortedRoles = RoleHelper.DisplayRoles
                .OrderByDescending(r => RoleHierarchy.GetRank(r.ToStringValue()))
                .ToList();

            for (int i = 0; i < sortedRoles.Count; i++)
            {
                var roleEnum = sortedRoles[i];
                string value = roleEnum.ToStringValue();

                var item = new CheckBoxListItem
                {
                    ID = i,
                    Display = value,
                    Label = roleEnum.GetDisplayName(),
                    IsChecked = userRoles.Contains(value),
                    IsEditable = allowedRoles.Contains(value)
                };

                vm.Roles.Add(item);
            }

            vm.Institution = mapper.Map<InstitutionVM>(await institutionRepository.GetAsync(user.InstitutionId));
            vm.SubSpeciality = mapper.Map<SubSpecialityVM>(await subSpecialityRepository.GetAsync(user.SubSpecialityId));
            if (vm.SubSpeciality != null)
                vm.Speciality = vm.SubSpeciality.Speciality;

            return vm;
        }

        private async Task<string> GetScopeTitleForUserAsync(ClaimsPrincipal user)
        {
            var currentUser = await userManager.GetUserAsync(User);

            if (user.IsInRole(Role.Administrator.ToStringValue()))
                return "Wombat Users";

            if (user.IsInRole(Role.InstitutionalAdmin.ToStringValue()))
                return $"Wombat Users at {currentUser.Institution?.Name}";

            if (user.IsInRole(Role.SpecialityAdmin.ToStringValue()))
                return $"Wombat Users at {currentUser.Institution?.Name} in {currentUser.SubSpeciality?.Speciality?.Name}";

            if (user.IsInRole(Role.SubSpecialityAdmin.ToStringValue()))
                return $"Womabt Users at {currentUser.Institution?.Name} in {currentUser.SubSpeciality?.Speciality?.Name}, {currentUser.SubSpeciality?.Name}";

            return "Wombat Users";
        }

        public async Task<List<WombatUserVM>> GetVMWithRoles(List<WombatUser> users)
        {
            List<WombatUserVM> VMList = new List<WombatUserVM>();
            foreach (var user in users)
            {
                VMList.Add(await GetVMWithRoles(user));
            }

            return VMList;
        }

        // GET: WombatUsers
        [Authorize(Policy = Claims.ManageUsers)]
        public async Task<IActionResult> Index()
        {
            var currentUser = await userManager.GetUserAsync(User);
            var roles = await userManager.GetRolesAsync(currentUser);

            IQueryable<WombatUser> query = userManager.Users
                .Where(u => u.Id != currentUser.Id); // Exclude current user

            if (roles.Contains(Role.Administrator.ToStringValue()))
            {
                // Global admin: no filter
            }
            else if (roles.Contains(Role.InstitutionalAdmin.ToStringValue()))
            {
                // Institutional admin: same institution
                query = query.Where(u => u.InstitutionId == currentUser.InstitutionId);
            }
            else if (roles.Contains(Role.SpecialityAdmin.ToStringValue()))
            {
                // Speciality admin: same institution + same speciality
                int? targetSpecialityId = currentUser.SubSpeciality?.SpecialityId;

                query = query.Where(u =>
                    u.InstitutionId == currentUser.InstitutionId &&
                    u.SubSpeciality != null &&
                    u.SubSpeciality.SpecialityId == targetSpecialityId);
            }
            else if (roles.Contains(Role.SubSpecialityAdmin.ToStringValue()))
            {
                // Subspeciality admin: same institution + same subspeciality
                query = query.Where(u =>
                    u.InstitutionId == currentUser.InstitutionId &&
                    u.SubSpecialityId == currentUser.SubSpecialityId);
            }
            else
            {
                return Forbid(); // No access
            }

            // Load related SubSpeciality and Speciality (for later mapping if needed)
            var users = await query
                .Include(u => u.SubSpeciality)
                    .ThenInclude(s => s.Speciality)
                .ToListAsync();

            var usersVM = await GetVMWithRoles(users);
            ViewData["UserScopeTitle"] = await GetScopeTitleForUserAsync(User);
            return View(usersVM);
        }

        // GET: WombatUsers/Details/5
        [Authorize(Policy = Claims.ManageUsers)]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var wombatUserVM = await GetVMWithRoles(await userManager.FindByIdAsync(id));
            if (wombatUserVM == null)
            {
                return NotFound();
            }

            return View(wombatUserVM);
        }

        private bool CanEditUser(WombatUser currentUser, WombatUser targetUser, IList<string> roles)
        {
            if (currentUser.Id == targetUser.Id)
                return false; // Don't allow editing yourself

            if (roles.Contains(Role.Administrator.ToStringValue()))
                return true;

            if (roles.Contains(Role.InstitutionalAdmin.ToStringValue()))
                return currentUser.InstitutionId == targetUser.InstitutionId;

            if (roles.Contains(Role.SpecialityAdmin.ToStringValue()))
            {
                return currentUser.InstitutionId == targetUser.InstitutionId &&
                       currentUser.SubSpeciality?.SpecialityId == targetUser.SubSpeciality?.SpecialityId;
            }

            if (roles.Contains(Role.SubSpecialityAdmin.ToStringValue()))
            {
                return currentUser.InstitutionId == targetUser.InstitutionId &&
                       currentUser.SubSpecialityId == targetUser.SubSpecialityId;
            }

            return false;
        }

        // GET: WombatUsers/Edit/5
        [Authorize(Policy = Claims.ManageUsers)]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var targetUser = await userManager.Users
                .Include(u => u.SubSpeciality)
                    .ThenInclude(s => s.Speciality)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (targetUser == null) return NotFound();

            var currentUser = await userManager.GetUserAsync(User);
            var roles = await userManager.GetRolesAsync(currentUser);

            if (!CanEditUser(currentUser, targetUser, roles))
            {
                return Forbid();
            }

            var wombatUserVM = await GetVMWithRoles(targetUser);
            return View(wombatUserVM);
        }


        // POST: WombatUsers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Claims.ManageUsers)]
        public async Task<IActionResult> Edit(string id, WombatUserVM wombatUserVM)
        {
            if (id != wombatUserVM.Id)
                return NotFound();

            var targetUser = await userManager.Users
                .Include(u => u.SubSpeciality)
                    .ThenInclude(s => s.Speciality)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (targetUser == null)
                return NotFound();

            var currentUser = await userManager.GetUserAsync(User);
            var currentUserRoles = await userManager.GetRolesAsync(currentUser);

            if (!CanEditUser(currentUser, targetUser, currentUserRoles))
                return Forbid();

            if (ModelState.IsValid)
            {
                try
                {
                    var allowedRoles = RoleHelper.DisplayRoles
                        .Where(r => currentUserRoles.Any(cur => RoleHierarchy.CanAssign(cur, r.ToStringValue())))
                        .Select(r => r.ToStringValue())
                        .ToHashSet();

                    var targetUserRoles = await userManager.GetRolesAsync(targetUser);

                    // Only remove roles the editor is allowed to manage
                    foreach (var role in targetUserRoles)
                    {
                        if (allowedRoles.Contains(role))
                            await userManager.RemoveFromRoleAsync(targetUser, role);
                    }

                    // Only assign allowed roles
                    foreach (var role in wombatUserVM.Roles.Where(r => r.IsChecked))
                    {
                        if (allowedRoles.Contains(role.Display))
                            await userManager.AddToRoleAsync(targetUser, role.Display);
                    }

                    await userManager.UpdateAsync(targetUser);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!WombatUserVMExists(wombatUserVM.Id))
                        return NotFound();

                    logger.LogError(ex, "Error updating user");
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(wombatUserVM);
        }

        private bool WombatUserVMExists(string id)
        {
            return userManager.Users.Any(e => e.Id == id);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Claims.ManageUsers)]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var targetUser = await userManager.Users
                .Include(u => u.SubSpeciality)
                    .ThenInclude(s => s.Speciality)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (targetUser == null)
                return NotFound();

            var currentUser = await userManager.GetUserAsync(User);
            var roles = await userManager.GetRolesAsync(currentUser);

            if (!CanEditUser(currentUser, targetUser, roles))
            {
                return Forbid();
            }

            // Prevent self-deletion
            if (currentUser.Id == targetUser.Id)
            {
                ModelState.AddModelError(string.Empty, "You cannot delete your own account.");
                return RedirectToAction(nameof(Index));
            }

            var userRoles = await userManager.GetRolesAsync(targetUser);
            await userManager.RemoveFromRolesAsync(targetUser, userRoles);
            await userManager.DeleteAsync(targetUser);

            return RedirectToAction(nameof(Index));
        }

    }
}
