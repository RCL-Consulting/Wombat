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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Contracts;
using Wombat.Common.Constants;
using Wombat.Data;
using Wombat.Common.Models;
using Wombat.Application.Repositories;
using Mono.TextTemplating;
using System.Collections.Generic;

namespace Wombat.Controllers
{
    [Authorize(Roles = Roles.Administrator)]
    public class AssessmentFormsController : Controller
    {
        private readonly IAssessmentFormRepository assessmentFormRepository;
        private readonly IOptionSetRepository optionSetRepository;
        private readonly IMapper mapper;

        public AssessmentFormsController( IAssessmentFormRepository assessmentFormRepository,
                                         IOptionSetRepository optionSetRepository,
                                         IMapper mapper )
        {
            this.assessmentFormRepository = assessmentFormRepository;
            this.optionSetRepository=optionSetRepository;
            this.mapper=mapper;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
        public IActionResult AddCriterion(AssessmentFormVM assessmentFormVM)
        {
            var Item = new OptionCriterionVM();
            Item.DisplayId = OptionCriterionVM.NextDisplayId++;
            Item.Rank = assessmentFormVM.OptionCriteria.Count;
            assessmentFormVM.OptionCriteria?.Add(Item);
            return PartialView("AssessmentForm", assessmentFormVM);
        }

        // GET: AssessmentForms
        public async Task<IActionResult> Index()
        {
            var Forms = mapper.Map<List<AssessmentFormVM>>(await assessmentFormRepository.GetAllAsync());
            return View(Forms);
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
            return View(assessmentFormVM);
        }

        // GET: AssessmentForms/Create
        public async Task<IActionResult> CreateAsync()
        {
            var assessmentFormVM = new AssessmentFormVM();
            OptionCriterionVM.OptionsSets = mapper.Map<List<OptionSetVM>>(await optionSetRepository.GetAllAsync());

            var Template = mapper.Map<AssessmentFormVM>(await assessmentFormRepository.GetAsync(AssessmentForm.kTemplateId));
            if (Template != null)
            {
                foreach (var optionCriterion in Template.OptionCriteria)
                {
                    assessmentFormVM.OptionCriteria.Add(
                        new OptionCriterionVM()
                        {
                            Description = optionCriterion.Description,
                            Rank = optionCriterion.Rank,
                            OptionSetId = optionCriterion.OptionSetId,
                            DisplayId = OptionCriterionVM.NextDisplayId++
                        });
                }
            }

            return View(assessmentFormVM);
        }

        // POST: AssessmentForms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssessmentFormVM assessmentFormVM)
        {
            if (ModelState.IsValid)
            {
                var assessmentForm = mapper.Map<AssessmentForm>(assessmentFormVM);
                await assessmentFormRepository.AddAsync(assessmentForm);
                return RedirectToAction(nameof(Index));
            }
            return View(assessmentFormVM);
        }

        // GET: AssessmentForms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var assessmentForm = await assessmentFormRepository.GetAsync(id);
            if (assessmentForm == null)
            {
                return NotFound();
            }

            OptionCriterionVM.OptionsSets = mapper.Map<List<OptionSetVM>>(await optionSetRepository.GetAllAsync());

            var assessmentFormVM = mapper.Map<AssessmentFormVM>(assessmentForm);
            return View(assessmentFormVM);
        }

        // POST: AssessmentForms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
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
            return View(assessmentFormVM);
        }

        // POST: AssessmentForms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await assessmentFormRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
