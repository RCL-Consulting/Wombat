using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Wombat.Data;
using Wombat.Models;

namespace Wombat.Controllers
{
    public class AssessmentCategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper mapper;

        public AssessmentCategoriesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            this.mapper=mapper;
        }

        // GET: AssessmentCategories
        public async Task<IActionResult> Index()
        {
            var categories = mapper.Map<List<AssessmentCategoryVM>>(await _context.AssessmentCategories.ToListAsync());
            return View(categories);
        }

        // GET: AssessmentCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assessmentCategory = await _context.AssessmentCategories
                .FirstOrDefaultAsync(m => m.Id == id);
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
                _context.Add(assessmentCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(assessmentCategoryVM);
        }

        // GET: AssessmentCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assessmentCategory = await _context.AssessmentCategories.FindAsync(id);
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
                    var assessmentCategory = mapper.Map<AssessmentCategoryVM>(assessmentCategoryVM);
                    _context.Update(assessmentCategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AssessmentCategoryExists(assessmentCategoryVM.Id))
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

        // GET: AssessmentCategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assessmentCategory = await _context.AssessmentCategories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (assessmentCategory == null)
            {
                return NotFound();
            }

            return View(assessmentCategory);
        }

        // POST: AssessmentCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var assessmentCategory = await _context.AssessmentCategories.FindAsync(id);
            if (assessmentCategory != null)
            {
                _context.AssessmentCategories.Remove(assessmentCategory);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AssessmentCategoryExists(int id)
        {
            return _context.AssessmentCategories.Any(e => e.Id == id);
        }
    }
}
