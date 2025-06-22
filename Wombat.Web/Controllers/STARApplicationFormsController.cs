using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NuGet.Protocol.Core.Types;
using Wombat.Application.Contracts;
using Wombat.Application.Repositories;
using Wombat.Common.Constants;
using Wombat.Common.Models;
using Wombat.Data;
using static Wombat.Common.Models.STARApplicationVM;

namespace Wombat.Web.Controllers
{
    [Authorize]
    public class STARApplicationFormsController : Controller
    {
        private readonly ISTARApplicationFormRepository formRepository;
        private readonly IOptionSetRepository optionSetRepository;
        private readonly IEPARepository epaRepository;
        private readonly ISpecialityRepository specialityRepository;
        private readonly ISubSpecialityRepository subSpecialityRepository;
        private readonly IMapper mapper;

        public STARApplicationFormsController( ISTARApplicationFormRepository formRepository,
                                               IOptionSetRepository optionSetRepository,
                                               IEPARepository epaRepository,
                                               ISpecialityRepository specialityRepository,
                                               ISubSpecialityRepository subSpecialityRepository,
                                               IMapper mapper)
        {
            this.formRepository = formRepository;
            this.optionSetRepository = optionSetRepository;
            this.epaRepository = epaRepository;
            this.specialityRepository = specialityRepository;
            this.subSpecialityRepository = subSpecialityRepository;
            this.mapper = mapper;
        }

        private async Task CreateViewBags( int? selectedSpecialityId = null, 
                                           int? selectedSubSpecialityId = null,
                                           int? selectedEPAId = null)
        {
            ViewBag.Specialities = (await specialityRepository.GetAllAsync())
                .Select(s => new SelectListItem
                {
                    Text = s.Name,
                    Value = s.Id.ToString(),
                    Selected = selectedSpecialityId.HasValue && s.Id == selectedSpecialityId
                }).ToList();

            var subspecialities = await subSpecialityRepository.GetAllAsync();
            ViewBag.Subspecialities = subspecialities
                .GroupBy(s => s.SpecialityId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(s => new SelectListItem
                    {
                        Text = s.Name,
                        Value = s.Id.ToString(),
                        Selected = selectedSubSpecialityId.HasValue && s.Id == selectedSubSpecialityId
                    }).ToList()
                );

            var epas = await epaRepository.GetAllAsync();
            ViewBag.EPAs = epas
                .GroupBy(e => e.SubSpecialityId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => new SelectListItem
                    {
                        Text = e.Name,
                        Value = e.Id.ToString(),
                        Selected = selectedEPAId.HasValue && e.Id == selectedEPAId
                    }).ToList()
                );

        }

        [HttpGet]
        [Authorize(Policy = Claims.ManageEPAs)]
        public async Task<IActionResult> Create()
        {
            ViewBag.OptionSets = mapper.Map<List<OptionSetVM>>(await optionSetRepository.GetAllAsync());

            var vm = new STARApplicationFormVM
            {
                CreatedOn = DateTime.UtcNow,
                STARItems = new List<STARItemVM>()
            };

            await CreateViewBags();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Claims.ManageEPAs)]
        public async Task<IActionResult> Create(STARApplicationFormVM model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.EPAs = new SelectList(await epaRepository.GetAllAsync(), "Id", "Name", model.EPAId);
                ViewBag.OptionSets = mapper.Map<List<OptionSetVM>>(await optionSetRepository.GetAllAsync());
                return View(model);
            }

            var newForm = new STARApplicationForm
            {
                Name = model.Name,
                EPAId = model.EPAId,
                CreatedOn = DateTime.UtcNow,
                IsPublished = model.IsPublished,
                STARItems = model.STARItems.Select((q, i) => new STARItem
                {
                    Rank = q.DisplayId,
                    Heading = q.Heading,
                    Description = q.Description,
                    OptionSetId = q.OptionSetId
                }).ToList()
            };

            await formRepository.AddAsync(newForm);
            return RedirectToAction(nameof(Index));
        }

        // GET: STARApplications/Details/{epaId}
        public async Task<IActionResult> Details(int id)
        {
            var form = await formRepository.GetAsync(id);
            if (form == null)
                return NotFound();

            var vm = new STARApplicationFormVM
            {
                Id = form.Id,
                Name = form.Name,
                CreatedOn = form.CreatedOn,
                IsPublished = form.IsPublished,
                EPAId = form.EPAId,
                EPAName = form.EPA?.Name ?? "",
                SpecialityName = form.EPA?.SubSpeciality?.Speciality?.Name ?? "",
                SubSpecialityName = form.EPA?.SubSpeciality?.Name ?? "",
                STARItems = form.STARItems.OrderBy(i => i.Rank).Select((q, i) => new STARItemVM
                {
                    Id = q.Id,
                    DisplayId = i + 1,
                    Heading = q.Heading,
                    Description = q.Description,
                    OptionSetId = q.OptionSetId,
                    OptionSet = mapper.Map<OptionSetVM>(q.OptionsSet)
                }).ToList()
            };

            return View(vm);
        }

        public async Task<IActionResult> Index()
        {
            var forms = await formRepository.GetAllAsync();
            var vm = forms.Select(f => new STARApplicationFormVM
            {
                Id = f.Id,
                Name = f.Name,
                CreatedOn = f.CreatedOn,
                IsPublished = f.IsPublished,
                EPAId = f.EPAId,
                EPAName = f.EPA?.Name ?? "",
                SpecialityName = f.EPA?.SubSpeciality?.Speciality?.Name ?? "",
                SubSpecialityName = f.EPA?.SubSpeciality?.Name ?? ""
            }).ToList();

            return View(vm);
        }

        [HttpGet]
        [Authorize(Policy = Claims.ManageEPAs)]
        public async Task<IActionResult> Edit(int id)
        {
            var form = await formRepository.GetAsync(id);
            if (form == null)
                return NotFound();

            var optionSets = await optionSetRepository.GetAllAsync();
            var selectList = optionSets
                .Select(o => new SelectListItem { Value = o.Id.ToString(), Text = o.Description })
                .ToList();

            var vm = new STARApplicationFormVM
            {
                Id = form.Id,
                Name = form.Name,
                CreatedOn = form.CreatedOn,
                IsPublished = form.IsPublished,
                EPAId = form.EPAId,
                EPAName = form.EPA?.Name ?? "",
                SpecialityName = form.EPA?.SubSpeciality?.Speciality?.Name ?? "",
                SubSpecialityName = form.EPA?.SubSpeciality?.Name ?? "",
                STARItems = form.STARItems.OrderBy(i => i.Rank).Select((q, i) => new STARItemVM
                {
                    Id = q.Id,
                    DisplayId = i + 1,
                    Heading = q.Heading,
                    Description = q.Description,
                    OptionSetId = q.OptionSetId,
                    AvailableOptionSets = selectList
                }).ToList()
            };

            vm.SpecialityId = form.EPA?.SubSpeciality?.SpecialityId ?? 0;
            vm.SubSpecialityId = form.EPA?.SubSpecialityId ?? 0;

            await CreateViewBags(
                selectedSpecialityId: form.EPA?.SubSpeciality?.SpecialityId,
                selectedSubSpecialityId: form.EPA?.SubSpecialityId,
                selectedEPAId: form.EPAId
            );
            ViewBag.OptionSets = mapper.Map<List<OptionSetVM>>(optionSets);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Claims.ManageEPAs)]
        public async Task<IActionResult> Edit(STARApplicationFormVM model)
        {
            var form = await formRepository.GetAsync(model.Id);
            if (form == null)
                return NotFound();

            // 1. Update form properties
            form.Name = model.Name;
            form.IsPublished = model.IsPublished;
            form.EPAId = model.EPAId;

            // 2. Build a lookup of existing items for comparison
            var existingItems = form.STARItems.ToDictionary(i => i.Id);
            var incomingIds = model.STARItems.Where(i => i.Id > 0).Select(i => i.Id).ToHashSet();

            // 3. Clear the collection (EF will re-track items added back)
            form.STARItems.Clear();

            foreach (var vmItem in model.STARItems)
            {
                STARItem item;
                if (vmItem.Id > 0 && existingItems.TryGetValue(vmItem.Id.Value, out var existing))
                {
                    item = existing;
                }
                else
                {
                    item = new STARItem();
                }

                item.Rank = vmItem.DisplayId;
                item.Heading = vmItem.Heading;
                item.Description = vmItem.Description;
                item.OptionSetId = vmItem.OptionSetId;
                item.FormId = form.Id;

                form.STARItems.Add(item);
            }

            // 4. Save the updated form and cascade changes
            await formRepository.UpdateAsync(form);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Claims.ManageEPAs)]
        public async Task<IActionResult> AddItem(STARApplicationFormVM model)
        {
            var nextId = model.STARItems.Any()
                ? model.STARItems.Max(q => q.DisplayId) + 1
                : 1;

            var optionSets = await optionSetRepository.GetAllAsync();
            var selectList = optionSets.Select(o => new SelectListItem
            {
                Value = o.Id.ToString(),
                Text = o.Description
            }).ToList();

            model.STARItems.Add(new STARItemVM
            {
                DisplayId = nextId,
                AvailableOptionSets = selectList
            });

            foreach (var q in model.STARItems)
                q.AvailableOptionSets = selectList;

            return PartialView("~/Views/STARApplicationForms/_STARApplicationFormsPartial.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Claims.ManageEPAs)]
        public async Task<IActionResult> DeleteItem(STARApplicationFormVM model, int displayId)
        {
            model.STARItems.RemoveAll(q => q.DisplayId == displayId);

            var optionSets = await optionSetRepository.GetAllAsync();
            var selectList = optionSets.Select(o => new SelectListItem
            {
                Value = o.Id.ToString(),
                Text = o.Description
            }).ToList();

            foreach (var q in model.STARItems)
                q.AvailableOptionSets = selectList;

            return PartialView("~/Views/STARApplicationForms/_STARApplicationFormsPartial.cshtml", model);
        }

        // POST: AssessmentForms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Claims.ManageEPAs)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await formRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
