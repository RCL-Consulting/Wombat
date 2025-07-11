﻿/*Copyright (C) 2024 RCL Consulting
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Contracts;
using Wombat.Data;
using Wombat.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Wombat.Common.Constants;
using Wombat.Application.Repositories;
using Serilog.Context;
using Newtonsoft.Json.Bson;

namespace Wombat.Controllers
{
    [Authorize]
    public class EPAsController: Controller
    {
        private readonly IEPARepository EPARepository;
        private readonly IAssessmentFormRepository assessmentFormRepository;
        private readonly ISpecialityRepository specialityRepository;
        private readonly ISubSpecialityRepository subSpecialityRepository;
        private readonly IOptionSetRepository optionSetRepository;
        private readonly IMapper mapper;
        //private readonly ApplicationDbContext Context;

        public EPAsController( /*ApplicationDbContext Context,*/
                               IEPARepository EPARepository,
                               IAssessmentFormRepository assessmentFormRepository,
                               ISpecialityRepository specialityRepository,
                               ISubSpecialityRepository subSpecialityRepository,
                               IOptionSetRepository optionSetRepository,
                               IMapper mapper )
        {
            this.EPARepository = EPARepository;
            this.assessmentFormRepository = assessmentFormRepository;
            this.specialityRepository = specialityRepository;
            this.subSpecialityRepository = subSpecialityRepository;
            this.optionSetRepository = optionSetRepository;
            this.mapper = mapper;
        }

        // GET: EPAs
        public async Task<IActionResult> Index()
        {
            var EPAs = mapper.Map<List<EPAVM>>(await EPARepository.GetAllAsync());
            return View(EPAs);
        }

        // GET: EPAs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var EPA = await EPARepository.GetAsync(id);
            if (EPA == null)
            {
                return NotFound();
            }

            var EPAVM = mapper.Map<EPAVM>(EPA);

            EPAVM.Speciality = EPAVM.SubSpeciality.Speciality;

            var Scale = await optionSetRepository.GetAsync(OptionSet.kEPAScaleId);
            if (Scale != null)
            {
                EPACurriculumVM.AvailableScaleOptions =
                Scale.Options
                .OrderBy(i => i.Rank)
                .Select(i => new SelectListItem(i.Rank.ToString() + "-" + i.Description, i.Id.ToString()))
                .ToList();
            }

            return View(EPAVM);
        }

        // GET: EPAs/Create
        [Authorize(Policy = Claims.ManageEPAs)]
        public async Task<IActionResult> Create()
        {
            //var Forms = mapper.Map<List<AssessmentFormVM>>(await assessmentFormRepository.GetAllAsync());
            //ViewBag.Forms = Forms;

            var model = new EPAVM
            {
                Specialities = mapper.Map<List<SpecialitySelectVM>>(await specialityRepository.GetAllAsync()),
                SubSpecialities = new List<SubSpecialitySelectVM>()
            };

            EPAVM.AvailableForms = mapper.Map<List<AssessmentFormVM>>(await assessmentFormRepository.GetAllAsync());
            ViewData["Forms"] = EPAVM.AvailableForms;

            var Scale = await optionSetRepository.GetAsync(OptionSet.kEPAScaleId);
            if (Scale != null)
            {
                EPACurriculumVM.AvailableScaleOptions =
                Scale.Options
                .OrderBy(i => i.Rank)
                .Select(i => new SelectListItem(i.Rank.ToString() + "-" + i.Description, i.Id.ToString()))              
                .ToList();
            }
            return View(model);
        }

        public async Task<IActionResult> GetSubOptions(int mainOptionId)
        {
            var speciality = await specialityRepository.GetAsync(mainOptionId);

            var subSpecialities = mapper.Map<List<SubSpecialitySelectVM>>(speciality?.SubSpecialities);

            return Json(subSpecialities);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Claims.ManageEPAs)]
        public IActionResult DeleteForm(EPAVM epaVM, int displayId)
        {
            ViewData.ModelState.Clear();//CanDeleteFromList
            var Item = epaVM.Forms?.FirstOrDefault(s => s.DisplayId == displayId);
            if (Item != null)
            {
                epaVM.Forms?.RemoveAll(s => s.DisplayId == displayId);
            }
            ViewData["Forms"] = EPAVM.AvailableForms;
            return PartialView("EPA", epaVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Claims.ManageEPAs)]
        public IActionResult AddForm(EPAVM epaVM)
        {
            var Item = new EPAFormVM();
            Item.DisplayId = EPAFormVM.NextDisplayId++;
            Item.EPAId = epaVM.Id;
            epaVM.Forms?.Add(Item);
            ViewData["Forms"] = EPAVM.AvailableForms;
            return PartialView("EPA", epaVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Claims.ManageEPAs)]
        public IActionResult DeleteCurriculum(EPAVM epaVM, int displayId)
        {
            ViewData.ModelState.Clear();//CanDeleteFromList
            var Item = epaVM.EPACurricula?.FirstOrDefault(s => s.DisplayId == displayId);
            if (Item != null)
            {
                epaVM.EPACurricula?.RemoveAll(s => s.DisplayId == displayId);
            }
            //ViewData["Forms"] = EPAVM.AvailableForms;
            return PartialView("EPACurriculum", epaVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Claims.ManageEPAs)]
        public IActionResult AddCurriculum(EPAVM epaVM)
        {
            var Item = new EPACurriculumVM();
            Item.DisplayId = EPACurriculumVM.NextDisplayId++;
            Item.EPAId = epaVM.Id;
            epaVM.EPACurricula?.Add(Item);
            //ViewData["Forms"] = EPAVM.AvailableForms;
            return PartialView("EPACurriculum", epaVM);
        }

        // POST: EPAs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Claims.ManageEPAs)]
        public async Task<IActionResult> Create(EPAVM EPAVM)
        {
            if (ModelState.IsValid)
            {
                var EPAContext = mapper.Map<EPA>(EPAVM);

                await EPARepository.AddAsync(EPAContext);
                return RedirectToAction(nameof(Index));
            }

            EPAVM.Specialities = mapper.Map<List<SpecialitySelectVM>>(await specialityRepository.GetAllAsync());
            var speciality = await specialityRepository.GetAsync(EPAVM.SpecialityId);
            if(speciality != null)
            {
                EPAVM.SubSpecialities = mapper.Map<List<SubSpecialitySelectVM>>(speciality?.SubSpecialities);
            }
            else
            {
                EPAVM.SubSpecialities = new List<SubSpecialitySelectVM>();
            }

            return View(EPAVM);
        }

        // GET: EPAs/Edit/5
        [Authorize(Policy = Claims.ManageEPAs)]
        public async Task<IActionResult> Edit(int? id)
        {
            var EPA = await EPARepository.GetAsync(id);
            if (EPA == null)
            {
                return NotFound();
            }

            var Forms = mapper.Map<List<AssessmentFormVM>>(await assessmentFormRepository.GetAllAsync());
            ViewBag.Forms = Forms;

            var EPAVM = mapper.Map<EPAVM>(EPA);
            EPAVM.SpecialityId = EPA.SubSpeciality?.SpecialityId ?? 0;

            EPAVM.Specialities = mapper.Map<List<SpecialitySelectVM>>(await specialityRepository.GetAllAsync());
            if(EPAVM.SpecialityId!=0)
                EPAVM.SubSpecialities = mapper.Map<List<SubSpecialitySelectVM>>(await subSpecialityRepository.GetSubSpecialitiesBySpecialityAsync(EPAVM.SpecialityId));
            else
                EPAVM.SubSpecialities = new List<SubSpecialitySelectVM>();

            EPAVM.Speciality = mapper.Map<SpecialityVM>(await specialityRepository.GetAsync(EPAVM.SpecialityId));

            EPAVM.AvailableForms = mapper.Map<List<AssessmentFormVM>>(await assessmentFormRepository.GetAllAsync());
            ViewData["Forms"] = EPAVM.AvailableForms;

            var Scale = await optionSetRepository.GetAsync(OptionSet.kEPAScaleId);
            if (Scale != null)
            {
                EPACurriculumVM.AvailableScaleOptions =
                Scale.Options
                .OrderBy(i => i.Rank)
                .Select(i => new SelectListItem(i.Rank.ToString() + "-" + i.Description, i.Id.ToString()))
                .ToList();
            }

            return View(EPAVM);
        }

        void ManualMap(EPAVM EPAVM, EPA EPA)
        {
            EPA.Name = EPAVM.Name;
            EPA.Description = EPAVM.Description;
            EPA.SubSpecialityId = EPAVM.SubSpecialityId;
            EPA.EPACurricula = mapper.Map<List<EPACurriculum>>(EPAVM.EPACurricula);

            var itemsToCompare = EPA.Forms
                .Where(existing => EPAVM.Forms.Any(newItem => newItem.Id == existing.Id))
                .ToList();
            foreach (var item in EPAVM.Forms)
            {
                var existing = EPA.Forms.FirstOrDefault(s => s.Id == item.Id);
                if (existing != null)
                {
                    existing.FormId = item.FormId;
                }
            }

            var itemsToRemove = EPA.Forms
                .Where(existing => !EPAVM.Forms.Any(newItem => newItem.Id == existing.Id))
                .ToList();
            foreach (var item in itemsToRemove)
            {
                EPA.Forms.Remove(item);
            }

            foreach (var newItem in EPAVM.Forms)
            {
                if (!EPA.Forms.Any(existing => existing.Id == newItem.Id))
                {
                    EPA.Forms.Add( new EPAForm
                    {
                        EPAId = EPA.Id,
                        FormId = newItem.FormId
                    });
                }
            }
        }

        // POST: EPAs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Claims.ManageEPAs)]
        public async Task<IActionResult> Edit(int id, EPAVM EPAVM)
        {
            if (id != EPAVM.Id)
            {
                return NotFound();
            }

            var EPA = await EPARepository.GetAsync(id);

            if (EPA==null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ManualMap(EPAVM, EPA);
                    await EPARepository.UpdateAsync(EPA);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await EPARepository.Exists(EPAVM.Id))
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

            ViewData["Forms"] = EPAVM.AvailableForms;
            return View(EPAVM);
        }

        // POST: EPAs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Claims.ManageEPAs)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await EPARepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
