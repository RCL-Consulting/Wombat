using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Contracts;
using Wombat.Common.Constants;
using Wombat.Data;
using Wombat.Common.Models;
using Wombat.Application.Repositories;

namespace Wombat.Controllers
{
    [Authorize(Roles = Roles.Administrator)]
    public class AssessmentTemplatesController : Controller
    {
        private readonly IAssessmentTemplateRepository assessmentTemplateRepository;
        private readonly IOptionSetRepository optionSetRepository;
        private readonly IMapper mapper;

        public AssessmentTemplatesController(IAssessmentTemplateRepository assessmentTemplateRepository,
                                               IOptionSetRepository optionSetRepository,
                                               IMapper mapper)
        {
            this.assessmentTemplateRepository=assessmentTemplateRepository;
            this.optionSetRepository=optionSetRepository;
            this.mapper=mapper;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCriterion(AssessmentTemplateVM assessmentTemplateVM, int displayId)
        {
            ViewData.ModelState.Clear();//CanDeleteFromList
            var Item = assessmentTemplateVM.OptionCriteria?.FirstOrDefault(s => s.DisplayId == displayId);
            if (Item != null && Item.CanEditAndDelete)
            {
                assessmentTemplateVM.OptionCriteria?.RemoveAll(s => s.DisplayId == displayId);
            }
            return PartialView("AssessmentTemplate", assessmentTemplateVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddCriterion(AssessmentTemplateVM assessmentTemplateVM)
        {
            var Item = new OptionCriterionVM();
            Item.DisplayId = OptionCriterionVM.NextDisplayId++;
            Item.Rank = assessmentTemplateVM.OptionCriteria.Count;
            assessmentTemplateVM.OptionCriteria?.Add(Item);
            return PartialView("AssessmentTemplate", assessmentTemplateVM);
        }

        // GET: AssessmentTemplates
        public async Task<IActionResult> Index()
        {
            var templates = mapper.Map<List<AssessmentTemplateVM>>(await assessmentTemplateRepository.GetAllAsync());
            return View(templates);
        }

        // GET: AssessmentTemplates/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var assessmentTemplate = await assessmentTemplateRepository.GetAsync(id);
            if (assessmentTemplate == null)
            {
                return NotFound();
            }

            var assessmentTemplateVM = mapper.Map<AssessmentTemplateVM>(assessmentTemplate);
            return View(assessmentTemplateVM);
        }

        // GET: AssessmentTemplates/Create
        public async Task<IActionResult> CreateAsync()
        {
            var assessmentTemplateVM = new AssessmentTemplateVM();
            OptionCriterionVM.OptionsSets = mapper.Map<List<OptionSetVM>>(await optionSetRepository.GetAllAsync());

            return View(assessmentTemplateVM);
        }

        // POST: AssessmentTemplates/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssessmentTemplateVM assessmentTemplateVM)
        {
            if (ModelState.IsValid)
            {
                var assessmentTemplate = mapper.Map<AssessmentTemplate>(assessmentTemplateVM);
                await assessmentTemplateRepository.AddAsync(assessmentTemplate);
                return RedirectToAction(nameof(Index));
            }
            return View(assessmentTemplateVM);
        }

        // GET: AssessmentTemplates/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var assessmentTemplate = await assessmentTemplateRepository.GetAsync(id);
            if (assessmentTemplate == null)
            {
                return NotFound();
            }

            OptionCriterionVM.OptionsSets = mapper.Map<List<OptionSetVM>>(await optionSetRepository.GetAllAsync());

            var assessmentTemplateVM = mapper.Map<AssessmentTemplateVM>(assessmentTemplate);
            return View(assessmentTemplateVM);
        }

        // POST: AssessmentTemplates/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AssessmentTemplateVM assessmentTemplateVM)
        {
            if (id != assessmentTemplateVM.Id)
            {
                return NotFound();
            }

            var assessmentTemplate = await assessmentTemplateRepository.GetAsync(id);

            if (assessmentTemplate==null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    mapper.Map(assessmentTemplateVM, assessmentTemplate);
                    await assessmentTemplateRepository.UpdateAsync(assessmentTemplate);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await assessmentTemplateRepository.Exists(assessmentTemplateVM.Id))
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
            return View(assessmentTemplateVM);
        }

        // POST: AssessmentTemplates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await assessmentTemplateRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
