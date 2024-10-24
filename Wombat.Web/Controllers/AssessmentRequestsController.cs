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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Contracts;
using Wombat.Application.Repositories;
using Wombat.Common.Constants;
using Wombat.Common.Models;
using Wombat.Data;

namespace Wombat.Web.Controllers
{
    public class AssessmentRequestsController : Controller
    {
        private readonly IMapper mapper;
        private readonly IAssessmentRequestRepository assessmentRequestRepository;
        private readonly IAssessmentFormRepository assessmentFormRepository;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IEPARepository epaRepository;
        private readonly UserManager<WombatUser> userManager;

        public AssessmentRequestsController( IMapper mapper,
                                             IAssessmentRequestRepository assessmentRequestRepository,
                                             IHttpContextAccessor httpContextAccessor,
                                             IEPARepository epaRepository,
                                             UserManager<WombatUser> userManager,
                                             IAssessmentFormRepository assessmentFormRepository)
        {
            this.assessmentRequestRepository = assessmentRequestRepository;
            this.epaRepository = epaRepository;
            this.httpContextAccessor = httpContextAccessor;
            this.mapper = mapper;
            this.userManager = userManager;
            this.assessmentFormRepository = assessmentFormRepository;
        }

        // GET: AssessmentRequests
        
        private bool UserIsAssessor()
        {
            if (httpContextAccessor.HttpContext == null)
                return false;

            var user = userManager.GetUserAsync(httpContextAccessor.HttpContext.User).Result;
            var roles = userManager.GetRolesAsync(user).Result;
            return roles.Contains(Roles.Assessor);
        }

        private bool UserIsTrainee()
        {
            if (httpContextAccessor.HttpContext == null)
                return false;

            var user = userManager.GetUserAsync(httpContextAccessor.HttpContext.User).Result;
            var roles = userManager.GetRolesAsync(user).Result;
            return roles.Contains(Roles.Trainee);
        }

        public async Task<IActionResult> Index(AssessmentRequestStatus requestStatus)
        {
            if (httpContextAccessor.HttpContext == null)
                return NotFound();

            var userId = userManager.GetUserId(httpContextAccessor.HttpContext.User);
            ViewBag.RequestStatus = requestStatus;

            if (UserIsAssessor())
            {
                if (requestStatus == AssessmentRequestStatus.Completed)
                {
                    var Requests = mapper.Map<List<AssessmentRequestVM>>(await assessmentRequestRepository.GetAssessorCompletedAssessments(userId));
                    ViewBag.Heading = "Completed assessments";
                    return View(Requests);
                }
                else if (requestStatus == AssessmentRequestStatus.Requested)
                {
                    var Requests = mapper.Map<List<AssessmentRequestVM>>(await assessmentRequestRepository.GetAssessorPendingRequests(userId));
                    ViewBag.Heading = "Pending requests";
                    return View(Requests);
                }
                else if (requestStatus == AssessmentRequestStatus.Declined)
                {
                    var Requests = mapper.Map<List<AssessmentRequestVM>>(await assessmentRequestRepository.GetAssessorDeclinedRequests(userId));
                    ViewBag.Heading = "Declined requests";
                    return View(Requests);
                }
                else if (requestStatus == AssessmentRequestStatus.Accepted)
                {
                    var Requests = mapper.Map<List<AssessmentRequestVM>>(await assessmentRequestRepository.GetAssessorPendingAssessments(userId));
                    ViewBag.Heading = "Pending assessments";
                    return View(Requests);
                }
            }
            else if (UserIsTrainee())
            {
                if (requestStatus == AssessmentRequestStatus.Completed)
                {
                    var Requests = mapper.Map<List<AssessmentRequestVM>>(await assessmentRequestRepository.GetTraineeCompletedAssessments(userId));
                    ViewBag.Heading = "Completed assessments";
                    return View(Requests);
                }
                else if (requestStatus == AssessmentRequestStatus.Requested)
                {
                    var Requests = mapper.Map<List<AssessmentRequestVM>>(await assessmentRequestRepository.GetTraineePendingRequests(userId));
                    ViewBag.Heading = "Pending requests";
                    return View(Requests);
                }
                else if (requestStatus == AssessmentRequestStatus.Declined)
                {
                    var Requests = mapper.Map<List<AssessmentRequestVM>>(await assessmentRequestRepository.GetTraineeDeclinedRequests(userId));
                    ViewBag.Heading = "Declined requests";
                    return View(Requests);
                }
                else if (requestStatus == AssessmentRequestStatus.Accepted)
                {
                    var Requests = mapper.Map<List<AssessmentRequestVM>>(await assessmentRequestRepository.GetTraineePendingAssessments(userId));
                    ViewBag.Heading = "Pending assessments";
                    return View(Requests);
                }
            }

            return NotFound();
        }

        // GET: AssessmentRequests/Details/5
        public async Task<IActionResult> Details(int? id, AssessmentRequestStatus requestStatus)
        {
            var request = await assessmentRequestRepository.GetAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            ViewBag.RequestStatus = requestStatus;
            var requestVM = mapper.Map<AssessmentRequestVM>(request);

            requestVM.EPA = mapper.Map<EPAVM>(await epaRepository.GetAsync(request.EPAId));

            if (httpContextAccessor.HttpContext == null)
                return NotFound();

            var trainee = await userManager.FindByIdAsync(request.TraineeId);
            requestVM.Trainee = mapper.Map<WombatUserVM>(trainee);

            return View(requestVM);
        }

        public async Task<IActionResult> DeclineRequest(int? id)
        {
            var request = await assessmentRequestRepository.GetAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            var requestVM = mapper.Map<AssessmentRequestVM>(request);

            requestVM.EPA = mapper.Map<EPAVM>(await epaRepository.GetAsync(request.EPAId));

            var trainee = await userManager.FindByIdAsync(request.TraineeId);
            requestVM.Trainee = mapper.Map<WombatUserVM>(trainee);

            return View(requestVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeclineRequest(int id, AssessmentRequestVM assessmentRequestVM)
        {
            if (id != assessmentRequestVM.Id)
            {
                return NotFound();
            }
            assessmentRequestVM.DateDeclined = DateTime.Now;

            var assessmentRequest = await assessmentRequestRepository.GetAsync(id);

            if (assessmentRequest == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    assessmentRequest.AssessorNotes = assessmentRequestVM.AssessorNotes;
                    assessmentRequest.DateDeclined = assessmentRequestVM.DateDeclined;
                    await assessmentRequestRepository.UpdateAsync(assessmentRequest);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await assessmentRequestRepository.Exists(assessmentRequestVM.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "Home");
            }
            return View(assessmentRequestVM);
        }

        public async Task<IActionResult> AcceptRequest(int? id)
        {
            var request = await assessmentRequestRepository.GetAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            var requestVM = mapper.Map<AssessmentRequestVM>(request);

            requestVM.EPA = mapper.Map<EPAVM>(await epaRepository.GetAsync(request.EPAId));

            var trainee = await userManager.FindByIdAsync(request.TraineeId);
            requestVM.Trainee = mapper.Map<WombatUserVM>(trainee);

            return View(requestVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptRequest(int id, AssessmentRequestVM assessmentRequestVM)
        {
            if (id != assessmentRequestVM.Id)
            {
                return NotFound();
            }
            assessmentRequestVM.DateAccepted = DateTime.Now;

            var assessmentRequest = await assessmentRequestRepository.GetAsync(id);

            if (assessmentRequest == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    assessmentRequest.AssessorNotes = assessmentRequestVM.AssessorNotes;
                    assessmentRequest.DateAccepted = assessmentRequestVM.DateAccepted;
                    await assessmentRequestRepository.UpdateAsync(assessmentRequest);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await assessmentRequestRepository.Exists(assessmentRequestVM.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "Home");
            }
            return View(assessmentRequestVM);
        }

        // GET: AssessmentRequests/Create
        public IActionResult Create()
        {
            return View();
        }

        public async Task<IActionResult> CreateForEPA(int Id)
        {
            var request = new AssessmentRequestVM();
            request.EPA = mapper.Map<EPAVM>(await epaRepository.GetAsync(Id));
            request.EPAId = Id;

            if (httpContextAccessor.HttpContext == null)
                return NotFound();

            var userId = userManager.GetUserId(httpContextAccessor.HttpContext.User);
            request.TraineeId = userId;

            request.Trainee = mapper.Map<WombatUserVM>(await userManager.GetUserAsync(httpContextAccessor.HttpContext.User));

            var assessors = await userManager.GetUsersInRoleAsync(Roles.Assessor);
            List<SelectListItem> assessorList = new List<SelectListItem>();
            foreach (var assessor in assessors)
            {
                
                if (assessor.InstitutionId == request.Trainee.InstitutionId)
                {
                    assessorList.Add(new SelectListItem(assessor.Name + " " + assessor.Surname +" ("+assessor.Email+")", assessor.Id));
                }
            }
            ViewBag.Assessors = assessorList;

            List<SelectListItem> epaList = (await epaRepository.GetEPAListBySubspeciality(request.Trainee.SubSpeciality.Id))
                .Select(s => new SelectListItem(s.Name, s.Id.ToString()))
                .ToList();
            ViewBag.EPAs = epaList;

            List<SelectListItem> formList = (await epaRepository.GetFormsByEPA(Id))
                .Select(s => new SelectListItem(s.Name, s.Id.ToString()))
                .ToList();
            ViewBag.Forms = formList;

            return View(request);
        }

        // POST: AssessmentRequests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateForEPA(AssessmentRequestVM assessmentRequestVM)
        {
            if (ModelState.IsValid)
            {
                assessmentRequestVM.Id = 0;
                assessmentRequestVM.EPA = null;
                assessmentRequestVM.DateRequested = DateTime.Now;
                var request = mapper.Map<AssessmentRequest>(assessmentRequestVM);
                await assessmentRequestRepository.AddAsync(request);
                return RedirectToAction("Index", "Home");
            }
            return View(assessmentRequestVM);
        }

        // GET: AssessmentRequests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var request = await assessmentRequestRepository.GetAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            var requestVM = mapper.Map<AssessmentRequestVM>(request);
            return View(requestVM);
        }

        // POST: AssessmentRequests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AssessmentRequestVM assessmentRequestVM)
        {
            if (id != assessmentRequestVM.Id)
            {
                return NotFound();
            }

            var assessmentRequest = await assessmentRequestRepository.GetAsync(id);

            if (assessmentRequest == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    mapper.Map(assessmentRequestVM, assessmentRequest);
                    await assessmentRequestRepository.UpdateAsync(assessmentRequest);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await assessmentRequestRepository.Exists(assessmentRequestVM.Id))
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
            return View(assessmentRequestVM);
        }

        // POST: AssessmentForms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await assessmentRequestRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
