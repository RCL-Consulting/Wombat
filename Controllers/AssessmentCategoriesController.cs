using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Wombat.Constants;
using Wombat.Contracts;
using Wombat.Data;
using Wombat.Models;
using Wombat.Repositories;

namespace Wombat.Controllers
{
    [Authorize(Roles = Roles.Administrator)]
    public class AssessmentCategoriesController : Controller
    {
        private readonly IAssessmentCategoryRepository assessmentCategoryRepository;
        private readonly IMapper mapper;

        public AssessmentCategoriesController(IAssessmentCategoryRepository assessmentCategoryRepository, IMapper mapper)
        {
            this.assessmentCategoryRepository=assessmentCategoryRepository;
            this.mapper=mapper;
        }

        // GET: AssessmentCategories
        public async Task<IActionResult> Index()
        {
            var categories = mapper.Map<List<AssessmentCategoryVM>>(await assessmentCategoryRepository.GetAllAsync());
            return View(categories);
        }

        // GET: AssessmentCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var assessmentCategory = await assessmentCategoryRepository.GetAsync(id);
            if (assessmentCategory == null)
            {
                return NotFound();
            }

            var assessmentCategoryVM = mapper.Map<AssessmentCategoryVM>(assessmentCategory);
            return View(assessmentCategoryVM);
        }

        // GET: AssessmentCategories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AssessmentCategories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssessmentCategoryVM assessmentCategoryVM)
        {
            if (ModelState.IsValid)
            {
                var assessmentCategory = mapper.Map<AssessmentCategory>(assessmentCategoryVM);
                await assessmentCategoryRepository.AddAsync(assessmentCategory);
                return RedirectToAction(nameof(Index));
            }
            return View(assessmentCategoryVM);
        }

        // GET: AssessmentCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var assessmentCategory = await assessmentCategoryRepository.GetAsync(id);
            if (assessmentCategory == null)
            {
                return NotFound();
            }

            var assessmentCategoryVM = mapper.Map<AssessmentCategoryVM>(assessmentCategory);
            return View(assessmentCategoryVM);
        }

        // POST: AssessmentCategories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AssessmentCategoryVM assessmentCategoryVM)
        {
            if (id != assessmentCategoryVM.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var assessmentCategory = mapper.Map<AssessmentCategory>(assessmentCategoryVM);
                    await assessmentCategoryRepository.UpdateAsync(assessmentCategory);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await assessmentCategoryRepository.Exists(assessmentCategoryVM.Id))
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
            return View(assessmentCategoryVM);
        }

        // POST: AssessmentCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await assessmentCategoryRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
