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
        private readonly IAssessmentFormRepository assessmentFormRepository;
        private readonly ISpecialityRepository specialityRepository;
        private readonly ISubSpecialityRepository subSpecialityRepository;
        private readonly IMapper mapper;

        public EPAsController( IEPARepository EPARepository,
                               IAssessmentFormRepository assessmentFormRepository,
                               ISpecialityRepository specialityRepository,
                               ISubSpecialityRepository subSpecialityRepository,
                               IMapper mapper )
        {
            this.EPARepository = EPARepository;
            this.assessmentFormRepository = assessmentFormRepository;
            this.specialityRepository = specialityRepository;
            this.subSpecialityRepository = subSpecialityRepository;
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

            return View(EPAVM);
        }

        // GET: EPAs/Create
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
        public IActionResult AddForm(EPAVM epaVM)
        {
            var Item = new EPAFormVM();
            Item.DisplayId = EPAFormVM.NextDisplayId++;
            Item.EPAId = epaVM.Id;
            epaVM.Forms?.Add(Item);
            ViewData["Forms"] = EPAVM.AvailableForms;
            return PartialView("EPA", epaVM);
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

            ViewData["Forms"] = EPAVM.AvailableForms;
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
