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
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Mono.TextTemplating;
using System.Collections.Generic;
using Wombat.Application.Contracts;
using Wombat.Application.Repositories;
using Wombat.Common.Constants;
using Wombat.Common.Models;
using Wombat.Data;
using static Wombat.Common.Models.AssessmentFormVM;

namespace Wombat.Controllers
{
    [Authorize]
    public class AssessmentFormsController : Controller
    {
        private readonly UserManager<WombatUser> userManager;
        private readonly IAssessmentFormRepository assessmentFormRepository;
        private readonly IOptionSetRepository optionSetRepository;
        private readonly IUserContextService userContext;
        private readonly IInstitutionRepository institutionRepo;
        private readonly ISpecialityRepository specialityRepo;
        private readonly ISubSpecialityRepository subSpecialityRepo;
        private readonly IEPARepository epaRepository;
        private readonly IMapper mapper;

        public AssessmentFormsController( UserManager<WombatUser> userManager,
                                          IAssessmentFormRepository assessmentFormRepository,
                                          IOptionSetRepository optionSetRepository,
                                          IUserContextService userContext,
                                          IInstitutionRepository institutionRepo,
                                          ISpecialityRepository specialityRepo,
                                          ISubSpecialityRepository subSpecialityRepo,
                                          IEPARepository epaRepository,
                                          IMapper mapper )
        {
            this.userManager = userManager;
            this.assessmentFormRepository = assessmentFormRepository;
            this.optionSetRepository=optionSetRepository;
            this.userContext = userContext;
            this.institutionRepo = institutionRepo;
            this.specialityRepo = specialityRepo;
            this.subSpecialityRepo = subSpecialityRepo;
            this.epaRepository = epaRepository;
            this.mapper=mapper;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Claims.ManageAssessmentForms)]
        public IActionResult DeleteCriterion(AssessmentFormVM assessmentFormVM, int displayId)
        {
            ViewData.ModelState.Clear();//CanDeleteFromList
            var Item = assessmentFormVM.OptionCriteria?.FirstOrDefault(s => s.DisplayId == displayId);
            if (Item != null && Item.CanEditAndDelete)
            {
                assessmentFormVM.OptionCriteria?.RemoveAll(s => s.DisplayId == displayId);
            }
            return PartialView("AssessmentForm", assessmentFormVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Claims.ManageAssessmentForms)]
        public IActionResult AddCriterion(AssessmentFormVM assessmentFormVM)
        {
            var Item = new OptionCriterionVM();
            Item.DisplayId = OptionCriterionVM.NextDisplayId++;
            Item.Rank = assessmentFormVM.OptionCriteria.Count;
            assessmentFormVM.OptionCriteria?.Add(Item);
            return PartialView("AssessmentForm", assessmentFormVM);
        }

        [HttpPost]
        [Authorize(Policy = Claims.ManageAssessmentForms)]
        public async Task<IActionResult> CloneCriteria(int templateFormId, [FromForm] AssessmentFormVM currentForm)
        {
            try
            {
                var template = await assessmentFormRepository.GetAsync(templateFormId);
                if (template == null || template.OptionCriteria == null)
                    return NotFound("Template form not found.");

                var currentUser = await userManager.GetUserAsync(User);
                var roles = await userManager.GetRolesAsync(currentUser);
                var allOptionSets = await optionSetRepository.GetScopedOptionSetsAsync(currentUser, roles);
                var optionSetVMs = mapper.Map<List<OptionSetVM>>(allOptionSets);

                // ✅ Assign OptionsSets to populate the dropdown
                OptionCriterionVM.OptionsSets = optionSetVMs;

                var clonedItems = template.OptionCriteria
                    .OrderBy(c => c.Rank)
                    .Select(c =>
                    {
                        var vm = mapper.Map<OptionCriterionVM>(c);
                        vm.Id = 0; // Reset ID for new items
                        vm.DisplayId = OptionCriterionVM.NextDisplayId++;
                        vm.CanEditAndDelete = true;

                        // Optionally assign OptionsSet if needed for details view
                        var set = optionSetVMs.FirstOrDefault(o => o.Id == c.OptionSetId);
                        if (set != null)
                            vm.OptionsSet = set;

                        return vm;
                    }).ToList();


                var mergedList = currentForm.OptionCriteria ?? new List<OptionCriterionVM>();
                mergedList.AddRange(clonedItems);

                for (int i = 0; i < mergedList.Count; i++)
                    mergedList[i].Rank = i + 1;

                currentForm.OptionCriteria = mergedList;

                return PartialView("_OptionCriterionListPartial", currentForm);
            }
            catch (Exception ex)
            {
                // Replace with your logging mechanism
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: AssessmentForms
        public async Task<IActionResult> Index()
        {
            var currentUser = await userManager.GetUserAsync(User);
            var roles = await userManager.GetRolesAsync(currentUser);
            var isGlobalAdmin = roles.Contains(Role.Administrator.ToStringValue());

            var scopedForms = await assessmentFormRepository.GetScopedFormsAsync(currentUser, roles);

            var formVMs = mapper.Map<List<AssessmentFormVM>>(scopedForms);

            foreach (var vm in formVMs)
            {
                // Global admins can only edit global forms (InstitutionId == null)
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

            return View(formVMs);
        }

        // GET: AssessmentForms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var assessmentForm = await assessmentFormRepository.GetAsync(id);
            if (assessmentForm == null)
            {
                return NotFound();
            }

            var assessmentFormVM = mapper.Map<AssessmentFormVM>(assessmentForm);

            var currentUser = await userManager.GetUserAsync(User);
            var roles = await userManager.GetRolesAsync(currentUser);
            var isGlobalAdmin = roles.Contains(Role.Administrator.ToStringValue());

            if (isGlobalAdmin)
            {
                assessmentFormVM.IsEditableByCurrentUser = assessmentFormVM.InstitutionId == null;
            }
            else if (
                roles.Contains(Role.InstitutionalAdmin.ToStringValue()) &&
                assessmentFormVM.InstitutionId != null &&
                currentUser.InstitutionId == assessmentFormVM.InstitutionId)
            {
                assessmentFormVM.IsEditableByCurrentUser = true;
            }
            else if (
                roles.Contains(Role.SpecialityAdmin.ToStringValue()) &&
                assessmentFormVM.InstitutionId != null &&
                currentUser.InstitutionId == assessmentFormVM.InstitutionId &&
                assessmentFormVM.SpecialityId != null &&
                currentUser.SubSpeciality?.SpecialityId == assessmentFormVM.SpecialityId)
            {
                assessmentFormVM.IsEditableByCurrentUser = true;
            }
            else if (
                roles.Contains(Role.SubSpecialityAdmin.ToStringValue()) &&
                assessmentFormVM.InstitutionId != null &&
                currentUser.InstitutionId == assessmentFormVM.InstitutionId &&
                assessmentFormVM.SubSpecialityId != null &&
                currentUser.SubSpecialityId == assessmentFormVM.SubSpecialityId)
            {
                assessmentFormVM.IsEditableByCurrentUser = true;
            }
            else
            {
                assessmentFormVM.IsEditableByCurrentUser = false;
            }

            if (assessmentFormVM.InstitutionId != null)
                assessmentFormVM.InstitutionName = (await institutionRepo.GetAsync(assessmentFormVM.InstitutionId)).Name;
            if(assessmentFormVM.SubSpecialityId!=null)
            {
                var subSpeciality = await subSpecialityRepo.GetAsync(assessmentFormVM.SubSpecialityId);
                if (subSpeciality != null)
                {
                    assessmentFormVM.SpecialityName = subSpeciality.Speciality.Name;
                    assessmentFormVM.SubSpecialityName = subSpeciality.Name;
                }
            }

            return View(assessmentFormVM);
        }

        private async Task PopulateAssessmentFormVMAsync(AssessmentFormVM vm)
        {
            var currentUser = await userManager.GetUserAsync(User);
            var roles = await userManager.GetRolesAsync(currentUser);

            if (currentUser.InstitutionId != null)
                currentUser.Institution = await institutionRepo.GetAsync(currentUser.InstitutionId);
            if (currentUser.SubSpecialityId != null)
                currentUser.SubSpeciality = await subSpecialityRepo.GetAsync(currentUser.SubSpecialityId);

            vm.AllSubSpecialities = (await subSpecialityRepo.GetAllAsync())
                .Select(s => new SubSpecialityOption
                {
                    Id = s.Id,
                    Name = s.Name,
                    SpecialityId = s.SpecialityId
                }).ToList();

            if (roles.Contains(Role.Administrator.ToStringValue()))
            {
                vm.Institutions = (await institutionRepo.GetAllAsync())
                    .Select(i => new SelectListItem { Text = i.Name, Value = i.Id.ToString() }).ToList();
                vm.Specialities = (await specialityRepo.GetAllAsync())
                    .Select(s => new SelectListItem { Text = s.Name, Value = s.Id.ToString() }).ToList();
                if (vm.SpecialityId != null)
                {
                    vm.SubSpecialities = (await subSpecialityRepo.GetAllAsync())
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
                vm.InstitutionId = currentUser.InstitutionId;
                vm.Institutions = new List<SelectListItem> {
                    new SelectListItem {
                        Value = currentUser.InstitutionId.ToString(),
                        Text = currentUser.Institution?.Name ?? "Institution"
                    }
                };
                vm.Specialities = (await specialityRepo.GetAllAsync())
                    .Select(s => new SelectListItem { Text = s.Name, Value = s.Id.ToString() }).ToList();
                if (vm.SpecialityId != null)
                {
                    vm.SubSpecialities = (await subSpecialityRepo.GetAllAsync())
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
                vm.InstitutionId = currentUser.InstitutionId;
                vm.Institutions = new List<SelectListItem> {
                    new SelectListItem {
                        Value = currentUser.InstitutionId.ToString(),
                        Text = currentUser.Institution?.Name ?? "Institution"
                    }
                };
                vm.SpecialityId = currentUser.SubSpeciality?.SpecialityId;
                vm.Specialities = new List<SelectListItem> {
                    new SelectListItem {
                        Value = currentUser.SubSpeciality?.SpecialityId.ToString(),
                        Text = currentUser.SubSpeciality?.Speciality?.Name ?? "Speciality"
                    }
                };
                if (vm.SpecialityId != null)
                {
                    vm.SubSpecialities = (await subSpecialityRepo.GetAllAsync())
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
                vm.InstitutionId = currentUser.InstitutionId;
                vm.Institutions = new List<SelectListItem> {
                    new SelectListItem {
                        Value = currentUser.InstitutionId.ToString(),
                        Text = currentUser.Institution?.Name ?? "Institution"
                    }
                };
                vm.SpecialityId = currentUser.SubSpeciality?.SpecialityId;
                vm.Specialities = new List<SelectListItem> {
                    new SelectListItem {
                        Value = currentUser.SubSpeciality?.SpecialityId.ToString(),
                        Text = currentUser.SubSpeciality?.Speciality?.Name ?? "Speciality"
                    }
                };
                vm.SubSpecialityId = currentUser.SubSpecialityId;
                vm.SubSpecialities = new List<SelectListItem> {
                    new SelectListItem {
                        Value = currentUser.SubSpecialityId.ToString(),
                        Text = currentUser.SubSpeciality?.Name ?? "SubSpeciality"
                    }
                };
            }

            vm.AllEPAs = (await epaRepository.GetAllAsync())
                .Select(e => new EPAOption
                {
                    Id = e.Id,
                    Name = e.Name,
                    SubSpecialityId = e.SubSpecialityId
                }).ToList();

            var allOptionSets = await optionSetRepository.GetScopedOptionSetsAsync(currentUser, roles);
            ViewBag.OptionSets = mapper.Map<List<OptionSetVM>>(allOptionSets);
            // ✅ Assign OptionsSets to populate the dropdown
            OptionCriterionVM.OptionsSets = ViewBag.OptionSets;

            var templates = await assessmentFormRepository.GetScopedFormsAsync(currentUser, roles);
            ViewBag.Templates = templates
                .Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name })
                .ToList();
        }


        // GET: AssessmentForms/Create
        [Authorize(Policy = Claims.ManageAssessmentForms)]
        public async Task<IActionResult> CreateAsync()
        {
            var vm = new AssessmentFormVM();
            await PopulateAssessmentFormVMAsync(vm);
            return View(vm);
        }

        // POST: AssessmentForms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Claims.ManageAssessmentForms)]
        public async Task<IActionResult> Create(AssessmentFormVM assessmentFormVM)
        {
            if (ModelState.IsValid)
            {
                var entity = mapper.Map<AssessmentForm>(assessmentFormVM);

                entity.InstitutionId = assessmentFormVM.InstitutionId;
                entity.SpecialityId = assessmentFormVM.SpecialityId;
                entity.SubSpecialityId = assessmentFormVM.SubSpecialityId;

                entity.Institution = null;
                entity.Speciality = null;
                entity.SubSpeciality = null;

                // Attach selected EPAs
                if (assessmentFormVM.SelectedEPAIds != null && assessmentFormVM.SelectedEPAIds.Any())
                {
                    entity.EPAs = assessmentFormVM.SelectedEPAIds
                        .Select(id => new EPAForm { EPAId = id })
                        .ToList();
                }

                await assessmentFormRepository.AddAsync(entity);
                return RedirectToAction(nameof(Index)); // Redirect on success
            }

            // Rehydrate dropdowns if ModelState is invalid
            assessmentFormVM.AllSubSpecialities = (await subSpecialityRepo.GetAllAsync())
                .Select(s => new SubSpecialityOption
                {
                    Id = s.Id,
                    Name = s.Name,
                    SpecialityId = s.SpecialityId
                }).ToList();

            assessmentFormVM.AllEPAs = (await epaRepository.GetAllAsync())
                .Select(e => new EPAOption
                {
                    Id = e.Id,
                    Name = e.Name,
                    SubSpecialityId = e.SubSpecialityId
                }).ToList();

            // You should also rehydrate Institution/Speciality/SubSpeciality dropdowns here based on the user role
            // (optionally move that logic to a private method to reuse from both GET and POST)

            var currentUser = await userManager.GetUserAsync(User);
            var roles = await userManager.GetRolesAsync(currentUser);
            var allOptionSets = await optionSetRepository.GetScopedOptionSetsAsync(currentUser, roles);
            ViewBag.OptionSets = mapper.Map<List<OptionSetVM>>(allOptionSets);

            var templates = await assessmentFormRepository.GetScopedFormsAsync(currentUser, roles);
            ViewBag.Templates = templates
                .Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name })
                .ToList();

            return View(assessmentFormVM);
        }

        // GET: AssessmentForms/Edit/5
        [Authorize(Policy = Claims.ManageAssessmentForms)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var assessmentForm = await assessmentFormRepository.GetAsync(id);
            if (assessmentForm == null)
                return NotFound();

            var vm = mapper.Map<AssessmentFormVM>(assessmentForm);
            await PopulateAssessmentFormVMAsync(vm);

            vm.SelectedEPAIds = assessmentForm.EPAs?.Select(e => e.EPAId).ToList() ?? new List<int>();

            return View(vm);
        }

        // POST: AssessmentForms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Claims.ManageAssessmentForms)]
        public async Task<IActionResult> Edit(int id, AssessmentFormVM assessmentFormVM)
        {
            if (id != assessmentFormVM.Id)
            {
                return NotFound();
            }

            var assessmentForm = await assessmentFormRepository.GetAsync(id);

            if (assessmentForm == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    mapper.Map(assessmentFormVM, assessmentForm);

                    assessmentForm.Institution = null;
                    assessmentForm.Speciality = null;
                    assessmentForm.SubSpeciality = null;

                    await assessmentFormRepository.UpdateAsync(assessmentForm);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await assessmentFormRepository.Exists(assessmentFormVM.Id))
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

            var currentUser = await userManager.GetUserAsync(User);
            var roles = await userManager.GetRolesAsync(currentUser);
            var allOptionSets = await optionSetRepository.GetScopedOptionSetsAsync(currentUser, roles);
            ViewBag.OptionSets = mapper.Map<List<OptionSetVM>>(allOptionSets);

            var templates = await assessmentFormRepository.GetScopedFormsAsync(currentUser, roles);
            ViewBag.Templates = templates
                .Where(f => f.Id != assessmentForm.Id) // optionally tag forms as templates
                .Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name })
                .ToList();

            return View(assessmentFormVM);
        }

        // POST: AssessmentForms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Claims.ManageAssessmentForms)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await assessmentFormRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
