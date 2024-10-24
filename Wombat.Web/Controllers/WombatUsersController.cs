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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wombat.Data;
using Wombat.Common.Models;
using Wombat.Application.Repositories;
using Microsoft.AspNetCore.Authorization;
using Wombat.Common.Constants;
using Wombat.Application.Contracts;

namespace Wombat.Controllers
{
    [Authorize(Roles = Roles.Administrator)]
    public class WombatUsersController : Controller
    {
        private readonly UserManager<WombatUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly ILogger<WombatUsersController> logger;
        private readonly IInstitutionRepository institutionRepository;
        private readonly ISpecialityRepository specialityRepository;
        private readonly ISubSpecialityRepository subSpecialityRepository;
        private readonly IMapper mapper;

        public WombatUsersController( UserManager<WombatUser> userManager,
                                      RoleManager<IdentityRole> roleManager,
                                      ILogger<WombatUsersController> logger,
                                      IInstitutionRepository institutionRepository,
                                      ISpecialityRepository specialityRepository,
                                      ISubSpecialityRepository subSpecialityRepository,
                                      IMapper mapper)
        {
            this.userManager=userManager;
            this.roleManager=roleManager;
            this.logger=logger;
            this.institutionRepository = institutionRepository;
            this.specialityRepository = specialityRepository;
            this.subSpecialityRepository = subSpecialityRepository;
            this.mapper=mapper;
        }

        public async Task<WombatUserVM> GetVMWithRoles(WombatUser user)
        {
            var allRoles = await roleManager.Roles.ToListAsync();
            var userRoles = await userManager.GetRolesAsync(user);
            var VM = mapper.Map<WombatUserVM>(user);

            for (int i = 0; i<allRoles.Count; i++)
            {
                string Name = allRoles[i].Name;
                var ListItem = new CheckBoxListItem();
                ListItem.ID = i;
                ListItem.Display = Name;
                ListItem.IsChecked = userRoles.Contains(Name);
                VM.Roles.Add(ListItem);
            }

            VM.Institution = mapper.Map<InstitutionVM>(await institutionRepository.GetAsync(user.InstitutionId));
            VM.SubSpeciality = mapper.Map<SubSpecialityVM>(await subSpecialityRepository.GetAsync(user.SubSpecialityId));
            if (VM.SubSpeciality != null)
                VM.Speciality = VM.SubSpeciality.Speciality;
            return VM;
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
        public async Task<IActionResult> Index()
        {
            var users = await userManager.Users.ToListAsync();
            var usersVM = await GetVMWithRoles(users);
            return View(usersVM);
        }

        // GET: WombatUsers/Details/5
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

        // GET: WombatUsers/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await userManager.FindByIdAsync(id);
            var wombatUserVM = await GetVMWithRoles(user);

            //await AddViewDataAsync();
            return View(wombatUserVM);
        }

        // POST: WombatUsers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, WombatUserVM wombatUserVM)
        {
            if (id != wombatUserVM.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await userManager.FindByIdAsync(id);
                    if (user == null)
                        return View(wombatUserVM);

                    var roles = await userManager.GetRolesAsync(user);
                    await userManager.RemoveFromRolesAsync(user, roles);
                    foreach (var role in wombatUserVM.Roles)
                    {
                        if (role.IsChecked)
                            await userManager.AddToRoleAsync(user, role.Display);
                    }

                    await userManager.UpdateAsync(user);
                }
                catch (DbUpdateConcurrencyException exception)
                {
                    if (!WombatUserVMExists(wombatUserVM.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        logger.LogError(exception, "Error updating user roles");
                        throw;
                    }
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
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user != null)
            {
                var roles = await userManager.GetRolesAsync(user);
                await userManager.RemoveFromRolesAsync(user, roles);

                await userManager.DeleteAsync(user);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
