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
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Contracts;
using Wombat.Application.Repositories;
using Wombat.Common.Constants;
using Wombat.Common.Models;
using Wombat.Data;

namespace Wombat.Controllers
{
    public static class RoleScopingHelper
    {
        public static void SetEditable(OptionSetVM vm, WombatUser currentUser, IList<string> roles)
        {
            var isGlobalAdmin = roles.Contains(Role.Administrator.ToStringValue());

            if (isGlobalAdmin)
            {
                vm.IsEditableByCurrentUser = vm.InstitutionId == null;
            }
            else if (
                roles.Contains(Role.InstitutionalAdmin.ToStringValue()) &&
                vm.InstitutionId != null &&
                currentUser.InstitutionId == vm.InstitutionId)
            {
                vm.IsEditableByCurrentUser = true;
            }
            else if (
                roles.Contains(Role.SpecialityAdmin.ToStringValue()) &&
                vm.InstitutionId != null &&
                currentUser.InstitutionId == vm.InstitutionId &&
                vm.SpecialityId != null &&
                currentUser.SubSpeciality?.SpecialityId == vm.SpecialityId)
            {
                vm.IsEditableByCurrentUser = true;
            }
            else if (
                roles.Contains(Role.SubSpecialityAdmin.ToStringValue()) &&
                vm.InstitutionId != null &&
                currentUser.InstitutionId == vm.InstitutionId &&
                vm.SubSpecialityId != null &&
                currentUser.SubSpecialityId == vm.SubSpecialityId)
            {
                vm.IsEditableByCurrentUser = true;
            }
            else
            {
                vm.IsEditableByCurrentUser = false;
            }
        }
    }

    [Authorize]
    public class OptionSetsController : Controller
    {
        private readonly IOptionSetRepository optionSetRepository;
        private readonly UserManager<WombatUser> userManager;
        private readonly IInstitutionRepository institutionRepository;
        private readonly ISpecialityRepository specialityRepository;
        private readonly ISubSpecialityRepository subSpecialityRepository;
        private readonly IMapper mapper;

        public OptionSetsController( IOptionSetRepository optionSetRepository,
                                     UserManager<WombatUser> userManager,
                                     IInstitutionRepository institutionRepository,
                                     ISpecialityRepository specialityRepository,
                                     ISubSpecialityRepository subSpecialityRepository,
                                     IMapper mapper )
        {
            this.optionSetRepository=optionSetRepository;
            this.userManager = userManager;
            this.institutionRepository = institutionRepository;
            this.specialityRepository = specialityRepository;
            this.subSpecialityRepository = subSpecialityRepository;
            this.mapper=mapper;
        }

        private async Task PopulateOptionSetVMAsync(OptionSetVM vm)
        {
            var user = await userManager.GetUserAsync(User);
            var roles = await userManager.GetRolesAsync(user);

            if (user.InstitutionId != null)
                user.Institution = await institutionRepository.GetAsync(user.InstitutionId);
            if (user.SubSpecialityId != null)
                user.SubSpeciality = await subSpecialityRepository.GetAsync(user.SubSpecialityId);

            vm.AllSubSpecialities = (await subSpecialityRepository.GetAllAsync())
                 .Select(s => new SubSpecialityOption
                 {
                     Id = s.Id,
                     Name = s.Name,
                     SpecialityId = s.SpecialityId
                 }).ToList();

            if (roles.Contains(Role.Administrator.ToStringValue()))
            {
                vm.Institutions = (await institutionRepository.GetAllAsync())
                    .Select(i => new SelectListItem { Text = i.Name, Value = i.Id.ToString() }).ToList();
                vm.Specialities = (await specialityRepository.GetAllAsync())
                    .Select(s => new SelectListItem { Text = s.Name, Value = s.Id.ToString() }).ToList();
                if (vm.SpecialityId != null)
                {
                    vm.SubSpecialities = (await subSpecialityRepository.GetAllAsync())
                        .Where(s => s.SpecialityId == vm.SpecialityId)
                        .Select(s => new SelectListItem { Text = s.Name, Value = s.Id.ToString() }).ToList();
                }
                else
                {
                    vm.SubSpecialities = new List<SelectListItem>(); // leave empty
                }
            }
            else if (roles.Contains(Role.InstitutionalAdmin.ToStringValue()))
            {
                vm.InstitutionId = user.InstitutionId;
                vm.Institutions = new List<SelectListItem> {
                    new SelectListItem {
                        Value = user.InstitutionId.ToString(),
                        Text = user.Institution?.Name ?? "Institution"
                    }
                };
                vm.Specialities = (await specialityRepository.GetAllAsync())
                    .Select(s => new SelectListItem { Text = s.Name, Value = s.Id.ToString() }).ToList();
                if (vm.SpecialityId != null)
                {
                    vm.SubSpecialities = (await subSpecialityRepository.GetAllAsync())
                        .Where(s => s.SpecialityId == vm.SpecialityId)
                        .Select(s => new SelectListItem { Text = s.Name, Value = s.Id.ToString() }).ToList();
                }
                else
                {
                    vm.SubSpecialities = new List<SelectListItem>(); // leave empty
                }
            }
            else if (roles.Contains(Role.SpecialityAdmin.ToStringValue()))
            {
                vm.InstitutionId = user.InstitutionId;
                vm.Institutions = new List<SelectListItem> {
                    new SelectListItem {
                        Value = user.InstitutionId.ToString(),
                        Text = user.Institution?.Name ?? "Institution"
                    }
                };
                vm.SpecialityId = user.SubSpeciality?.SpecialityId;
                vm.Specialities = new List<SelectListItem> {
                    new SelectListItem {
                        Value = user.SubSpeciality?.SpecialityId.ToString(),
                        Text = user.SubSpeciality?.Speciality?.Name ?? "Speciality"
                    }
                };
                if (vm.SpecialityId != null)
                {
                    vm.SubSpecialities = (await subSpecialityRepository.GetAllAsync())
                        .Where(s => s.SpecialityId == vm.SpecialityId)
                        .Select(s => new SelectListItem { Text = s.Name, Value = s.Id.ToString() }).ToList();
                }
                else
                {
                    vm.SubSpecialities = new List<SelectListItem>(); // leave empty
                }
            }
            else if (roles.Contains(Role.SubSpecialityAdmin.ToStringValue()))
            {
                vm.InstitutionId = user.InstitutionId;
                vm.Institutions = new List<SelectListItem> {
                    new SelectListItem {
                        Value = user.InstitutionId.ToString(),
                        Text = user.Institution?.Name ?? "Institution"
                    }
                };
                vm.SpecialityId = user.SubSpeciality?.SpecialityId;
                vm.Specialities = new List<SelectListItem> {
                    new SelectListItem {
                        Value = user.SubSpeciality?.SpecialityId.ToString(),
                        Text = user.SubSpeciality?.Speciality?.Name ?? "Speciality"
                    }
                };
                vm.SubSpecialityId = user.SubSpecialityId;
                vm.SubSpecialities = new List<SelectListItem> {
                    new SelectListItem {
                        Value = user.SubSpecialityId.ToString(),
                        Text = user.SubSpeciality?.Name ?? "SubSpeciality"
                    }
                };
            }
        }

        // GET: OptionSets
        [Authorize(Policy = Claims.ManageAssessmentForms)]
        public async Task<IActionResult> Index()
        {
            var currentUser = await userManager.GetUserAsync(User);
            var roles = await userManager.GetRolesAsync(currentUser);

            var optionSets = await optionSetRepository.GetScopedOptionSetsAsync(currentUser, roles);
            var optionSetVMs = mapper.Map<List<OptionSetVM>>(optionSets);

            foreach (var vm in optionSetVMs)
            {
                RoleScopingHelper.SetEditable(vm, currentUser, roles);
            }

            return View(optionSetVMs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Claims.ManageAssessmentForms)]
        public IActionResult DeleteOption(OptionSetVM optionSetVM, int displayId)
        {
            ViewData.ModelState.Clear();//CanDeleteFromList
            var Item = optionSetVM.Options?.FirstOrDefault(s => s.DisplayId == displayId);
            if (Item != null && Item.CanEditAndDelete)
            {
                optionSetVM.Options?.RemoveAll(s => s.DisplayId == displayId);
            }
            return PartialView("OptionSet", optionSetVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Claims.ManageAssessmentForms)]
        public IActionResult AddOption(OptionSetVM optionSetVM)
        {
            var Item = new OptionVM();
            Item.DisplayId = OptionVM.NextDisplayId++;
            Item.Rank = optionSetVM.Options.Count;
            optionSetVM.Options?.Add(Item);
            return PartialView("OptionSet", optionSetVM);
        }

        // GET: OptionSets/Details/5
        [Authorize(Policy = Claims.ManageAssessmentForms)]
        public async Task<IActionResult> Details(int? id)
        {
            var optionSet = await optionSetRepository.GetAsync(id);
            if (optionSet == null)
            {
                return NotFound();
            }

            var currentUser = await userManager.GetUserAsync(User);
            var roles = await userManager.GetRolesAsync(currentUser);

            var optionSetVM = mapper.Map<OptionSetVM>(optionSet);

            RoleScopingHelper.SetEditable(optionSetVM, currentUser, roles);

            if (optionSetVM.InstitutionId != null)
                optionSetVM.InstitutionName = (await institutionRepository.GetAsync(optionSetVM.InstitutionId)).Name;
            if (optionSetVM.SubSpecialityId != null)
            {
                var subSpeciality = await subSpecialityRepository.GetAsync(optionSetVM.SubSpecialityId);
                if (subSpeciality != null)
                {
                    optionSetVM.SpecialityName = subSpeciality.Speciality.Name;
                    optionSetVM.SubSpecialityName = subSpeciality.Name;
                }
            }

            return View(optionSetVM);
        }

        // GET: OptionSets/Create
        [Authorize(Policy = Claims.ManageAssessmentForms)]
        public async Task<IActionResult> CreateAsync()
        {
            var vm = new OptionSetVM();
            await PopulateOptionSetVMAsync(vm);
            return View(vm);
        }

        // POST: OptionSets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Claims.ManageAssessmentForms)]
        public async Task<IActionResult> Create(OptionSetVM optionSetVM)
        {
            if (ModelState.IsValid)
            {
                var optionSet = mapper.Map<Wombat.Data.OptionSet>(optionSetVM);
                if (optionSet.Options != null)
                {
                    foreach (var option in optionSet.Options)
                        option.OptionSet = optionSet;
                }

                optionSet.Institution = null;
                optionSet.Speciality = null;
                optionSet.SubSpeciality = null;

                await optionSetRepository.AddAsync(optionSet);
                return RedirectToAction(nameof(Index));
            }

            await PopulateOptionSetVMAsync(optionSetVM);
            return View(optionSetVM);
        }

        // GET: OptionSets/Edit/5
        [Authorize(Policy = Claims.ManageAssessmentForms)]
        public async Task<IActionResult> Edit(int? id)
        {
            var optionSet = await optionSetRepository.GetAsync(id);
            if (optionSet == null)
                return NotFound();

            var vm = mapper.Map<OptionSetVM>(optionSet);
            await PopulateOptionSetVMAsync(vm);
            return View(vm);
        }

        // POST: OptionSets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Claims.ManageAssessmentForms)]
        public async Task<IActionResult> Edit(int id, OptionSetVM optionSetVM)
        {
            if (id != optionSetVM.Id)
                return NotFound();

            var optionSet = await optionSetRepository.GetAsync(id);
            if (optionSet == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    mapper.Map(optionSetVM, optionSet);
                    await optionSetRepository.UpdateAsync(optionSet);
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await optionSetRepository.Exists(optionSetVM.Id))
                        return NotFound();
                    else
                        throw;
                }
            }

            await PopulateOptionSetVMAsync(optionSetVM);
            return View(optionSetVM);
        }

        // POST: OptionSets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Claims.ManageAssessmentForms)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await optionSetRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
