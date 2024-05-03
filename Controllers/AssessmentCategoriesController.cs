using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Contracts;
using Wombat.Common.Constants;
using Wombat.Data;
using Wombat.Common.Models;

namespace Wombat.Controllers
{
    [Authorize(Roles = Roles.Administrator)]
    public class AssessmentCategoriesController : Controller
    {
        private readonly IAssessmentCategoryRepository assessmentCategoryRepository;
        private readonly IOptionSetRepository optionSetRepository;
        private readonly IMapper mapper;

        public AssessmentCategoriesController(IAssessmentCategoryRepository assessmentCategoryRepository,
                                               IOptionSetRepository optionSetRepository,
                                               IMapper mapper)
        {
            this.assessmentCategoryRepository=assessmentCategoryRepository;
            this.optionSetRepository=optionSetRepository;
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
        public async Task<IActionResult> CreateAsync()
        {
            var assessmentCategoryVM = new AssessmentCategoryVM();
            assessmentCategoryVM.OptionSets = mapper.Map<List<OptionSetVM>>(await optionSetRepository.GetAllAsync());

            return View(assessmentCategoryVM);
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

            var assessmentCategory = await assessmentCategoryRepository.GetAsync(id);

            if (assessmentCategory==null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    mapper.Map(assessmentCategoryVM, assessmentCategory);
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
