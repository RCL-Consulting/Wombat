using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Wombat.Data;
using Wombat.Contracts;
using AutoMapper;
using Wombat.Models;
using Wombat.Repositories;
using Microsoft.AspNetCore.Identity;
using Wombat.Constants;

namespace Wombat.Controllers
{
    public class LoggedAssessmentsController : Controller
    {
        private readonly UserManager<WombatUser> userManager;
        private readonly ILoggedAssessmentRepository loggedAssessmentRepository;
        private readonly IAssessmentContextRepository assessmentContextRepository;
        private readonly IMapper mapper;

        public LoggedAssessmentsController( UserManager<WombatUser> userManager, 
                                            ILoggedAssessmentRepository loggedAssessmentRepository,
                                            IAssessmentContextRepository assessmentContextRepository,
                                            IMapper mapper )
        {
            this.userManager=userManager;
            this.loggedAssessmentRepository=loggedAssessmentRepository;
            this.assessmentContextRepository=assessmentContextRepository;
            this.mapper=mapper;
        }

        // GET: LoggedAssessments
        public async Task<IActionResult> Index()
        {
            var loggedAssessment = mapper.Map<List<LoggedAssessmentVM>>(await loggedAssessmentRepository.GetAllAsync());
            return View(loggedAssessment);
        }

        // GET: LoggedAssessments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var loggedAssessment = await loggedAssessmentRepository.GetAsync(id);
            if (loggedAssessment == null)
            {
                return NotFound();
            }

            var loggedAssessmentVM = mapper.Map<LoggedAssessmentVM>(loggedAssessment);
            return View(loggedAssessmentVM);

            //var loggedAssessment = await _context.LoggedAssessments
            //    .Include(l => l.AssessmentContext)
            //    .Include(l => l.Assessor)
            //    .Include(l => l.Trainee)
            //    .FirstOrDefaultAsync(m => m.Id == id);
        }

        public void AddViewData()
        {
            var assessmentContexts = mapper.Map<List<AssessmentContextVM>>(assessmentContextRepository.GetAllAsync().Result);
            ViewData["AssessmentContext"] = new SelectList(assessmentContexts, "Id", "Description");

            var assessors = userManager.GetUsersInRoleAsync(Roles.Assessor).Result;
            ViewData["Assessor"] = new SelectList(assessors, "Id", "UserName");

            var trainees = userManager.GetUsersInRoleAsync(Roles.Trainee).Result;
            ViewData["Trainee"] = new SelectList(trainees, "Id", "UserName");
        }

        // GET: LoggedAssessments/Create
        public async Task<IActionResult> Create()
        {
            AddViewData();
            return View();
        }

        // POST: LoggedAssessments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( LoggedAssessmentVM loggedAssessmentVM )
        {
            if (ModelState.IsValid)
            {
                var loggedAssessment = mapper.Map<LoggedAssessment>(loggedAssessmentVM);

                await loggedAssessmentRepository.AddAsync(loggedAssessment);
                return RedirectToAction(nameof(Index));
            }

            AddViewData();
            return View(loggedAssessmentVM);
        }

        // GET: LoggedAssessments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var loggedAssessment = await loggedAssessmentRepository.GetAsync(id);
            if (loggedAssessment == null)
            {
                return NotFound();
            }

            var loggedAssessmentVM = mapper.Map<LoggedAssessmentVM>(loggedAssessment);

            AddViewData();
            return View(loggedAssessmentVM);
        }

        // POST: LoggedAssessments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit( int id, LoggedAssessmentVM loggedAssessmentVM )
        {
            if (id != loggedAssessmentVM.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var loggedAssessment = mapper.Map<LoggedAssessment>(loggedAssessmentVM);
                    await loggedAssessmentRepository.UpdateAsync(loggedAssessment);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await loggedAssessmentRepository.Exists(loggedAssessmentVM.Id))
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

            AddViewData();
            return View(loggedAssessmentVM);
        }

        // POST: LoggedAssessments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await loggedAssessmentRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
