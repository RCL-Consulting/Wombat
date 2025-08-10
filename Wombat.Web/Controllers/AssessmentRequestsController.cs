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
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Wombat.Application.Contracts;
using Wombat.Application.Repositories;
using Wombat.Application.Services;
using Wombat.Common.Constants;
using Wombat.Common.Models;
using Wombat.Data;
using Wombat.Services;

namespace Wombat.Web.Controllers
{
    [Authorize]
    public class AssessmentRequestsController : Controller
    {
        private readonly IMapper mapper;
        private readonly IAssessmentRequestRepository assessmentRequestRepository;
        private readonly IAssessmentFormRepository assessmentFormRepository;
        private readonly IEmailSender emailSender;
        private readonly IWebHostEnvironment environment;
        private readonly IAssessmentWorkflowService assessmentWorkflowService;
        private readonly IAssessmentEventRepository assessmentEventRepository;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IEPARepository epaRepository;
        private readonly UserManager<WombatUser> userManager;

        public AssessmentRequestsController( IMapper mapper,
                                             IAssessmentRequestRepository assessmentRequestRepository,
                                             IHttpContextAccessor httpContextAccessor,
                                             IEPARepository epaRepository,
                                             UserManager<WombatUser> userManager,
                                             IAssessmentFormRepository assessmentFormRepository,
                                             IEmailSender emailSender,
                                             IWebHostEnvironment environment,
                                             IAssessmentWorkflowService assessmentWorkflowService,
                                             IAssessmentEventRepository assessmentEventRepository )
        {
            this.assessmentRequestRepository = assessmentRequestRepository;
            this.epaRepository = epaRepository;
            this.httpContextAccessor = httpContextAccessor;
            this.mapper = mapper;
            this.userManager = userManager;
            this.assessmentFormRepository = assessmentFormRepository;
            this.emailSender = emailSender;
            this.environment = environment;
            this.assessmentWorkflowService = assessmentWorkflowService;
            this.assessmentEventRepository = assessmentEventRepository;
        }
        
        private bool UserIsAssessor()
        {
            if (httpContextAccessor.HttpContext == null)
                return false;

            var user = userManager.GetUserAsync(httpContextAccessor.HttpContext.User).Result;
            var roles = userManager.GetRolesAsync(user).Result;
            return roles.Contains(Role.Assessor.ToStringValue());
        }

        private bool UserIsTrainee()
        {
            if (httpContextAccessor.HttpContext == null)
                return false;

            var user = userManager.GetUserAsync(httpContextAccessor.HttpContext.User).Result;
            var roles = userManager.GetRolesAsync(user).Result;
            return roles.Contains(Role.Trainee.ToStringValue());
        }

        public async Task<IActionResult> Index(AssessmentRequestStatus requestStatus)
        {
            if (httpContextAccessor.HttpContext == null)
                return NotFound();

            var userId = userManager.GetUserId(httpContextAccessor.HttpContext.User);

            Expression<Func<AssessmentRequest, bool>> isAssessor = r => r.AssessorId == userId;
            Expression<Func<AssessmentRequest, bool>> isTrainee = r => r.TraineeId == userId;

            if (UserIsAssessor())
            {             
                if (requestStatus == AssessmentRequestStatus.Completed)
                {
                    var Requests = mapper.Map<List<AssessmentRequestVM>>(await assessmentRequestRepository.GetCompletedAssessmentsAsync(isAssessor));
                    ViewBag.Heading = "Completed assessments";
                    return View(Requests);
                }
                else if (requestStatus == AssessmentRequestStatus.Requested)
                {
                    var Requests = mapper.Map<List<AssessmentRequestVM>>(await assessmentRequestRepository.GetPendingRequestsAsync(isAssessor));
                    ViewBag.Heading = "Pending requests";
                    return View(Requests);
                }
                else if (requestStatus == AssessmentRequestStatus.Declined)
                {
                    var Requests = mapper.Map<List<AssessmentRequestVM>>(await assessmentRequestRepository.GetDeclinedRequestsAsync(isAssessor));
                    ViewBag.Heading = "Declined requests";
                    return View(Requests);
                }
                else if (requestStatus == AssessmentRequestStatus.Accepted)
                {
                    var Requests = mapper.Map<List<AssessmentRequestVM>>(await assessmentRequestRepository.GetPendingAssessmentsAsync(isAssessor));
                    ViewBag.Heading = "Pending assessments";
                    return View(Requests);
                }
            }
            else if (UserIsTrainee())
            {
                if (requestStatus == AssessmentRequestStatus.Completed)
                {
                    var Requests = mapper.Map<List<AssessmentRequestVM>>(await assessmentRequestRepository.GetCompletedAssessmentsAsync(isTrainee));
                    ViewBag.Heading = "Completed assessments";
                    return View(Requests);
                }
                else if (requestStatus == AssessmentRequestStatus.Requested)
                {
                    var Requests = mapper.Map<List<AssessmentRequestVM>>(await assessmentRequestRepository.GetPendingRequestsAsync(isTrainee));
                    ViewBag.Heading = "Pending requests";
                    return View(Requests);
                }
                else if (requestStatus == AssessmentRequestStatus.Declined)
                {
                    var Requests = mapper.Map<List<AssessmentRequestVM>>(await assessmentRequestRepository.GetDeclinedRequestsAsync(isTrainee));
                    ViewBag.Heading = "Declined requests";
                    return View(Requests);
                }
                else if (requestStatus == AssessmentRequestStatus.Accepted)
                {
                    var Requests = mapper.Map<List<AssessmentRequestVM>>(await assessmentRequestRepository.GetPendingAssessmentsAsync(isTrainee));
                    ViewBag.Heading = "Pending assessments";
                    return View(Requests);
                }
            }

            return NotFound();
        }

        // GET: AssessmentRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var request = await assessmentRequestRepository.GetAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            var requestVM = mapper.Map<AssessmentRequestVM>(request);

            requestVM.EPA = mapper.Map<EPAVM>(await epaRepository.GetAsync(request.EPAId));

            if (httpContextAccessor.HttpContext == null)
                return NotFound();

            var trainee = await userManager.FindByIdAsync(request.TraineeId);
            requestVM.Trainee = mapper.Map<WombatUserVM>(trainee);

            requestVM.Events = mapper.Map<List<AssessmentEventVM>>(await assessmentEventRepository.GetEventsForRequestAsync(request.Id));

            return View(requestVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int id, AssessmentRequestVM model)
        {
            if (string.IsNullOrWhiteSpace(model.ActionComment))
                return RedirectToAction(nameof(Details), new { id = model.Id, anchor = "events" });

            // create AssessmentEvent (Type = CommentAdded), set Actor = current user, Timestamp = UtcNow
            await assessmentWorkflowService.AddCommentToRequestAsync(model.Id, userManager.GetUserId(httpContextAccessor.HttpContext.User), model.ActionComment, Request);

            return RedirectToAction(nameof(Details), new { id = model.Id, anchor = "events" });
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

            requestVM.Events = mapper.Map<List<AssessmentEventVM>>(await assessmentEventRepository.GetEventsForRequestAsync(request.Id));

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

            if (ModelState.IsValid)
            {
                try
                {
                    await assessmentWorkflowService.DeclineRequestAsync( assessmentRequestVM, 
                                                                         userManager.GetUserId(httpContextAccessor.HttpContext.User),
                                                                         Request );
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

        private void GetCAddalendarEvent( AssessmentRequestVM assessmentRequest,
                                          StringBuilder calendar )
        {
            string name = "";
            if (UserIsAssessor())
                name = assessmentRequest.Trainee.DisplayName;
            else if (UserIsTrainee())
                name = assessmentRequest.Assessor.DisplayName;

            string text = assessmentRequest.EPA?.Name + " Assessment (" + name + ")";

            calendar.AppendLine("BEGIN:VEVENT");
            calendar.AppendLine($"UID:{assessmentRequest.Id}@wombat.app");
            calendar.AppendLine($"DTSTAMP:{DateTime.UtcNow:yyyyMMddTHHmmssZ}");
            calendar.AppendLine($"DTSTART:{assessmentRequest.AssessmentDate:yyyyMMddTHHmmssZ}");
            calendar.AppendLine($"DTEND:{assessmentRequest.AssessmentDate:yyyyMMddTHHmmssZ}");
            calendar.AppendLine($"SUMMARY:{text}");
            calendar.AppendLine("END:VEVENT");
        }

        public async Task<IActionResult> ExportCalendarEntry(int? id)
        {
            var request = await assessmentRequestRepository.GetAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            var requestVM = mapper.Map<AssessmentRequestVM>(request);

            var calendar = new StringBuilder();
            calendar.AppendLine("BEGIN:VCALENDAR");
            calendar.AppendLine("VERSION:2.0");
            calendar.AppendLine("PRODID:-//Wombat//Assessment Calendar//EN");

            GetCAddalendarEvent(requestVM, calendar);

            calendar.AppendLine("END:VCALENDAR");

            return File(Encoding.UTF8.GetBytes(calendar.ToString()), "text/calendar", "calendar.ics");
        }

        public async Task<IActionResult> ExportCalendarEntries()
        {
            if (httpContextAccessor.HttpContext == null)
                return NotFound();

            var userId = userManager.GetUserId(httpContextAccessor.HttpContext.User);

            Expression<Func<AssessmentRequest, bool>> predicate;

            if (UserIsAssessor())
                predicate = r => r.AssessorId == userId;
            else if (UserIsTrainee())
                predicate = r => r.TraineeId == userId;
            else
                return NotFound();

            List<AssessmentRequestVM> PendingAssessments = mapper.Map<List<AssessmentRequestVM>>(await assessmentRequestRepository.GetPendingAssessmentsAsync(predicate));
            
            var calendar = new StringBuilder();
            calendar.AppendLine("BEGIN:VCALENDAR");
            calendar.AppendLine("VERSION:2.0");
            calendar.AppendLine("PRODID:-//Wombat//Assessment Calendar//EN");

            foreach (var requestVM in PendingAssessments)
            {
                GetCAddalendarEvent(requestVM, calendar);
            }

            calendar.AppendLine("END:VCALENDAR");

            return File(Encoding.UTF8.GetBytes(calendar.ToString()), "text/calendar", "calendar.ics");
        }

        [HttpGet]
        public async Task<IActionResult> CancelRequest(int id)
        {
            var vm = mapper.Map<AssessmentRequestVM>(await assessmentRequestRepository.GetAsync(id)); // or map manually
            if (vm == null) return NotFound();

            // Only allowed when currently Accepted (pending assessment)
            if (vm.Status != AssessmentRequestStatus.Accepted)
                return BadRequest("Only accepted requests can be cancelled.");

            // Only the trainee who created it (or an admin) can cancel
            var userId = userManager.GetUserId(User);
            var allowed = User.IsInRole(Role.Administrator.ToStringValue()) ||
                   vm.TraineeId == userId ||
                   vm.AssessorId == userId;
            if (!allowed) return Forbid();

            // Load events for context
            vm.Events = mapper.Map<List<AssessmentEventVM>>(await assessmentEventRepository.GetEventsForRequestAsync(id));
            return View(vm); // views/AssessmentRequests/Cancel.cshtml
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelRequest(AssessmentRequestVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var userId = userManager.GetUserId(User);
            await assessmentWorkflowService.CancelRequestAsync(vm, userId, Request);

            // Return to Accepted list (now it will drop out)
            return RedirectToAction(nameof(Index), new { requestStatus = AssessmentRequestStatus.Accepted });
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

            requestVM.Events = mapper.Map<List<AssessmentEventVM>>(await assessmentEventRepository.GetEventsForRequestAsync(request.Id));

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

            if (ModelState.IsValid)
            {
                try
                {
                    await assessmentWorkflowService.AcceptRequestAsync( assessmentRequestVM,
                                                                        userManager.GetUserId(httpContextAccessor.HttpContext.User),
                                                                        Request);
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

            var assessors = await userManager.GetUsersInRoleAsync(Role.Assessor.ToStringValue());
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
        [Authorize(Policy = Claims.RequestAssessment)]
        public async Task<IActionResult> CreateForEPA(AssessmentRequestVM assessmentRequestVM)
        {
            if (!ModelState.IsValid)
                return View(assessmentRequestVM);

            if(httpContextAccessor == null || httpContextAccessor.HttpContext == null)
                return NotFound();

            assessmentRequestVM.Id = 0;
            assessmentRequestVM.EPA = null;
            assessmentRequestVM.DateRequested = DateTime.Now;

            var request = mapper.Map<AssessmentRequest>(assessmentRequestVM);

            await assessmentWorkflowService.CreateRequestAsync( assessmentRequestVM, 
                                                                userManager.GetUserId(httpContextAccessor.HttpContext.User),
                                                                Request );

            return RedirectToAction("Index", "Home");
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

        // GET: AssessmentRequests/RescheduleRequest/5
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> RescheduleRequest(int id)
        {
            var req = await assessmentRequestRepository.GetAsync(id);
            if (req == null) return NotFound();

            var userId = userManager.GetUserId(User);
            var isParty = req.TraineeId == userId || req.AssessorId == userId || User.IsInRole(RoleStrings.Administrator);
            if (!isParty) return Forbid();

            // Only pending items (Requested or Accepted) may be rescheduled
            if (req.Status != AssessmentRequestStatus.Requested && req.Status != AssessmentRequestStatus.Accepted)
                return BadRequest("Only requested or accepted items can be rescheduled.");

            var vm = new RescheduleVM
            {
                Id = req.Id,
                Status = req.Status,
                CurrentAssessmentDate = req.AssessmentDate,
                NewAssessmentDate = req.AssessmentDate ?? DateTime.Now.AddDays(1),
                IsTrainee = req.TraineeId == userId,
                IsAssessor = req.AssessorId == userId
            };

            return View(vm); // Views/AssessmentRequests/Reschedule.cshtml
        }

        // POST: AssessmentRequests/RescheduleRequest
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RescheduleRequest(RescheduleVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var actorId = userManager.GetUserId(User);

            // Optionally extend your service to accept a message; if not, it already logs old→new.
            await assessmentWorkflowService.RescheduleRequestAsync(
                requestId: vm.Id,
                newAssessmentDateLocal: vm.NewAssessmentDate,
                comment: vm.Message, // can be null
                actorId: actorId,
                httpRequest: Request
            );

            return RedirectToAction(nameof(Details), new { id = vm.Id });
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
