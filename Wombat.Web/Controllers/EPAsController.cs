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

namespace Wombat.Controllers
{
    [Authorize(Roles = Roles.Administrator)]
    public class EPAsController: Controller
    {
        private readonly IEPARepository EPARepository;
        private readonly IAssessmentTemplateRepository assessmentTemplateRepository;
        private readonly IMapper mapper;

        public EPAsController( IEPARepository EPARepository,
                               IAssessmentTemplateRepository assessmentTemplateRepository,
                               IMapper mapper )
        {
            this.EPARepository = EPARepository;
            this.assessmentTemplateRepository = assessmentTemplateRepository;
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
            return View(EPAVM);
        }

        // GET: EPAs/Create
        public async Task<IActionResult> Create()
        {
            var Templates = mapper.Map<List<AssessmentTemplateVM>>(await assessmentTemplateRepository.GetAllAsync());
            ViewBag.Templates = Templates;
            return View();
        }

        // POST: EPAs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EPAVM EPAVM)
        {
            if (ModelState.IsValid)
            {
                var EPAContext = mapper.Map<EPA>(EPAVM);

                await EPARepository.AddAsync(EPAContext);
                return RedirectToAction(nameof(Index));
            }

            var Templates = mapper.Map<List<AssessmentTemplateVM>>(await assessmentTemplateRepository.GetAllAsync());
            ViewBag.Templates = Templates;
            return View(EPAVM);
        }

        // GET: EPAs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var EPA = await EPARepository.GetAsync(id);
            if (EPA == null)
            {
                return NotFound();
            }

            var Templates = mapper.Map<List<AssessmentTemplateVM>>(await assessmentTemplateRepository.GetAllAsync());
            ViewBag.Templates = Templates;

            var EPAVM = mapper.Map<EPAVM>(EPA);
            return View(EPAVM);
        }

        // POST: EPAs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
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
                    mapper.Map(EPAVM, EPA);
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

            var assessmentTemplates = mapper.Map<List<AssessmentTemplateVM>>(await assessmentTemplateRepository.GetAllAsync());
            ViewData["AssessmentTemplate"] = new SelectList(assessmentTemplates, "Id", "Name");
            return View(EPAVM);
        }

        // POST: EPAs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await EPARepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
