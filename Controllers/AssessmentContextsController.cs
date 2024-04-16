using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using Wombat.Contracts;
using Wombat.Data;
using Wombat.Models;
using Wombat.Repositories;

namespace Wombat.Controllers
{
    public class AssessmentContextsController : Controller
    {
        private readonly IAssessmentContextRepository assessmentContextRepository;
        private readonly IAssessmentCategoryRepository assessmentCategoryRepository;
        private readonly IMapper mapper;

        public AssessmentContextsController( IAssessmentContextRepository assessmentContextRepository,
                                             IAssessmentCategoryRepository assessmentCategoryRepository,
                                             IMapper mapper )
        {
            this.assessmentContextRepository=assessmentContextRepository;
            this.assessmentCategoryRepository=assessmentCategoryRepository;
            this.mapper=mapper;
        }

        // GET: AssessmentContexts
        public async Task<IActionResult> Index()
        {
            var assessmentContexts = mapper.Map<List<AssessmentContextVM>>(await assessmentContextRepository.GetAllAsync());
            return View(assessmentContexts);
        }

        // GET: AssessmentContexts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var assessmentContext = await assessmentContextRepository.GetAsync(id);
            if (assessmentContext == null)
            {
                return NotFound();
            }

            var assessmentContextVM = mapper.Map<AssessmentContextVM>(assessmentContext);
            return View(assessmentContextVM);
        }

        // GET: AssessmentContexts/Create
        public async Task<IActionResult> Create()
        {
            var assessmentCategories = mapper.Map<List<AssessmentCategoryVM>>(await assessmentCategoryRepository.GetAllAsync());
            ViewData["AssessmentCategory"] = new SelectList(assessmentCategories, "Id", "Name");
            return View();
        }

        // POST: AssessmentContexts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssessmentContextVM assessmentContextVM)
        {
            if (ModelState.IsValid)
            {
                var assessmentContext = mapper.Map<AssessmentContext>(assessmentContextVM);

                await assessmentContextRepository.AddAsync(assessmentContext);
                return RedirectToAction(nameof(Index));
            }

            var assessmentCategories = mapper.Map<List<AssessmentCategoryVM>>(await assessmentCategoryRepository.GetAllAsync());
            ViewData["AssessmentCategory"] = new SelectList(assessmentCategories, "Id", "Name");
            return View(assessmentContextVM);
        }

        // GET: AssessmentContexts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var assessmentContext = await assessmentContextRepository.GetAsync(id);
            if (assessmentContext == null)
            {
                return NotFound();
            }

            var optionSetVM = mapper.Map<OptionSetVM>(assessmentContext);

            var assessmentCategories = mapper.Map<List<AssessmentCategoryVM>>(await assessmentCategoryRepository.GetAllAsync());
            ViewData["AssessmentCategory"] = new SelectList(assessmentCategories, "Id", "Name");
            return View(assessmentContext);
        }

        // POST: AssessmentContexts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AssessmentContextVM assessmentContextVM)
        {
            if (id != assessmentContextVM.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var assessmentContext = mapper.Map<AssessmentContext>(assessmentContextVM);
                    await assessmentContextRepository.UpdateAsync(assessmentContext);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await assessmentContextRepository.Exists(assessmentContextVM.Id))
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

            var assessmentCategories = mapper.Map<List<AssessmentCategoryVM>>(await assessmentCategoryRepository.GetAllAsync());
            ViewData["AssessmentCategory"] = new SelectList(assessmentCategories, "Id", "Name");
            return View(assessmentContextVM);
        }

        // POST: AssessmentContexts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await assessmentContextRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
