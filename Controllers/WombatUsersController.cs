using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Wombat.Constants;
using Wombat.Data;
using Wombat.Models;

namespace Wombat.Controllers
{
    public class WombatUsersController : Controller
    {
        private readonly UserManager<WombatUser> userManager;
        private readonly IMapper mapper;

        public WombatUsersController( UserManager<WombatUser> userManager,
                                      IMapper mapper )
        {
            this.userManager=userManager;
            this.mapper=mapper;
        }

        // GET: WombatUsers
        public async Task<IActionResult> Index()
        {
            var users = mapper.Map<List<WombatUserVM>>(await userManager.Users.ToListAsync());
            return View(users);
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

            var wombatUserVM = mapper.Map<WombatUserVM>(await userManager.FindByIdAsync(id));
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
                    if (wombatUserVM.Trainee)
                        await userManager.AddToRoleAsync(user, Roles.Trainee);
                    else
                    {
                        if (wombatUserVM.Assessor)
                            await userManager.AddToRoleAsync(user, Roles.Assessor);
                        if (wombatUserVM.Administrator)
                            await userManager.AddToRoleAsync(user, Roles.Administrator);
                        if (wombatUserVM.Coordinator)
                            await userManager.AddToRoleAsync(user, Roles.Coordinator);
                    }  
                    
                    await userManager.AddToRoleAsync(user, "NewRole");
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
