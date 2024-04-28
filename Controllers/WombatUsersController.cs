using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Configuration.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Wombat.Constants;
using Wombat.Data;
using Wombat.Models;
using Wombat.Repositories;

namespace Wombat.Controllers
{
    public class WombatUsersController : Controller
    {
        private readonly UserManager<WombatUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IMapper mapper;

        public WombatUsersController( UserManager<WombatUser> userManager,
                                      RoleManager<IdentityRole> roleManager,
                                      IMapper mapper )
        {
            this.userManager=userManager;
            this.roleManager=roleManager;
            this.mapper=mapper;
        }

        public async Task<WombatUserVM> GetVMWithRoles(WombatUser user)
        {
            var allRoles = await roleManager.Roles.ToListAsync();
            var userRoles = await userManager.GetRolesAsync(user);
            var VM = mapper.Map<WombatUserVM>(user);

            for(int i = 0; i<allRoles.Count; i++)
            {
                string Name = allRoles[i].Name;
                var ListItem = new CheckBoxListItem();
                ListItem.ID = i;
                ListItem.Display = Name;
                ListItem.IsChecked = userRoles.Contains(Name);
                VM.Roles.Add(ListItem);
            }
            return VM;
        }

        public async Task<List<WombatUserVM>> GetVMWithRoles( List<WombatUser> users)
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

            var wombatUserVM = mapper.Map<WombatUserVM>(await userManager.FindByIdAsync(id));
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
                    if(user == null) 
                        return View(wombatUserVM);

                    var roles = await userManager.GetRolesAsync(user);
                    await userManager.RemoveFromRolesAsync(user, roles);
                    foreach (var role in wombatUserVM.Roles)
                    {
                        if(role.IsChecked)
                            await userManager.AddToRoleAsync(user, role.Display);
                    }
                    
                    await userManager.UpdateAsync(user);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WombatUserVMExists(wombatUserVM.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
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
    }
}
