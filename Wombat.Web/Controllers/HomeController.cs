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
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Wombat.Application.Contracts;
using Wombat.Application.Repositories;
using Wombat.Common.Constants;
using Wombat.Common.Models;
using Wombat.Data;
using static Wombat.Data.WombatUser;

namespace Wombat.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<WombatUser> userManager;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ISubSpecialityRepository subSpecialityRepository;
        private readonly ISpecialityRepository specialityRepository;
        private readonly IInstitutionRepository institutionRepository;
        private readonly IAssessmentFormRepository assessmentFormRepository;
        private readonly IEPARepository EPARepository;
        private readonly IAssessmentRequestRepository assessmentRequestRepository;
        private readonly ILoggedAssessmentRepository loggedAssessmentRepository;
        private readonly IRegistrationInvitationRepository registrationInvitationRepository;
        private readonly IUserContextService userContext;
        private readonly IMapper mapper;

        public HomeController( UserManager<WombatUser> userManager,
                               IHttpContextAccessor httpContextAccessor,
                               ISubSpecialityRepository subSpecialityRepository,
                               ISpecialityRepository specialityRepository,
                               ILogger<HomeController> logger,
                               IMapper mapper,
                               IInstitutionRepository institutionRepository,
                               IAssessmentFormRepository assessmentFormRepository,
                               IEPARepository EPARepository,
                               IAssessmentRequestRepository assessmentRequestRepository,
                               ILoggedAssessmentRepository loggedAssessmentRepository,
                               IRegistrationInvitationRepository registrationInvitationRepository, 
                               IUserContextService userContext  )
        {
            this.userManager = userManager;
            this.httpContextAccessor = httpContextAccessor;
            this.subSpecialityRepository = subSpecialityRepository;
            this.specialityRepository = specialityRepository;
            this.mapper = mapper;
            _logger = logger;
            this.institutionRepository = institutionRepository;
            this.assessmentFormRepository = assessmentFormRepository;
            this.EPARepository = EPARepository;
            this.assessmentRequestRepository = assessmentRequestRepository;
            this.loggedAssessmentRepository = loggedAssessmentRepository;
            this.registrationInvitationRepository = registrationInvitationRepository;
            this.userContext = userContext;
        }

        private int GetMonthsInTraining(DateTime startDate)
        {
            var now = DateTime.UtcNow;
            return ((now.Year - startDate.Year) * 12) + now.Month - startDate.Month;
        }

        [AllowAnonymous]
        public IActionResult Landing()
        {
            if (User.Identity?.IsAuthenticated ?? false)
                return RedirectToAction("Index");

            return View(); // synchronous
        }

        [Authorize]
        public async Task<IActionResult> IndexAsync()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return View();

            if (httpContextAccessor.HttpContext == null)
                return NotFound();

            var userId = userManager.GetUserId(httpContextAccessor.HttpContext.User);
            var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext.User);
            var roles = await userManager.GetRolesAsync(user);
            
            var dashboard = new DashboardVM
            {
                User = mapper.Map<WombatUserVM>(user),
                EPAList = new List<EPAVM>()
            };

            // Step 1: Load all pending users (from DB)
            var potentialPendingUsers = await userManager.Users
                .Where(u => u.ApprovalStatus == eApprovalStatus.Pending &&
                            u.InstitutionId == user.InstitutionId &&
                            u.SubSpecialityId == user.SubSpecialityId)
                .ToListAsync();

            // Step 2: Filter in-memory by role
            var pendingUsers = new List<WombatUser>();
            foreach (var u in potentialPendingUsers)
            {
                if (await userManager.IsInRoleAsync(u, "PendingTrainee"))
                    pendingUsers.Add(u);
            }

            // 2. Get all institution and subspeciality names (optional: filter down)
            var institutions = (await institutionRepository.GetAllAsync()).ToDictionary(i => i.Id, i => i.Name);
            var recentInstitutions = (await institutionRepository.GetAllAsync())
                .OrderByDescending(i => i.DateCreated) // assuming you have a DateCreated field
                .Take(3)
                .Select(i => new InstitutionVM
                {
                    Id = i.Id,
                    Name = i.Name,
                    Logo = i.Logo
                }).ToList();
            dashboard.RecentInstitutions = recentInstitutions;
            var subspecialities = (await subSpecialityRepository.GetAllAsync()).ToDictionary(s => s.Id, s => s.Name);

            // 3. Map to view model
            var pendingVMs = pendingUsers.Select(u => new WombatUserVM
            {
                Id = u.Id,
                Name = u.Name,
                Surname = u.Surname,
                Email = u.Email,
                //InstitutionName = institutions.TryGetValue(u.InstitutionId, out var instName) ? instName : "Unknown",
                //SubspecialityName = subspecialities.TryGetValue(u.SubSpecialityId.Value, out var subName) ? subName : "Unknown"
            }).ToList();

            // 4. Assign to the dashboard
            dashboard.Coordinator = new CoordinatorDashboardVM
            {
                PendingTrainees = pendingVMs
            };

            if (roles.Contains(Role.Trainee.ToStringValue()))
            {
                dashboard.User.SubSpeciality = mapper.Map<SubSpecialityVM>(
                    await subSpecialityRepository.GetAsync(user.SubSpecialityId)
                );

                if (dashboard.User.SubSpeciality == null)
                    return NotFound();

                dashboard.User.Speciality = dashboard.User.SubSpeciality.Speciality;

                var EPAList = await EPARepository.GetEPAListBySubspeciality(dashboard.User.SubSpeciality.Id);
                if (EPAList != null)
                {
                    dashboard.EPAList = mapper.Map<List<EPAVM>>(EPAList);
                    List<int> EPAIds = EPAList.Select(e => e.Id).ToList();

                    dashboard.TotalAssessmentsPerEPA = await loggedAssessmentRepository.GetTotalAssessmentsPerEPAByTrainee(EPAIds, userId);
                    dashboard.VisibleAssessmentsPerEPA = await loggedAssessmentRepository.GetVisibleAssessmentsPerEPAByTrainee(EPAIds, userId);

                    // Load completed assessments and extract scores (Rank)
                    var completedAssessments = await assessmentRequestRepository.GetTraineeCompletedAssessments(userId);

                    var scoredAssessments = completedAssessments
                        .Where(a => a.LoggedAssessment?.OptionCriterionResponses != null)
                        .Select(a =>
                        {
                            var score = a.LoggedAssessment.OptionCriterionResponses
                                .FirstOrDefault(r => r.Criterion?.OptionSetId == 2)?.Option?.Rank ?? 0;

                            return new
                            {
                                a.EPAId,
                                Score = score,
                                Date = a.CompletionDate ?? DateTime.MinValue
                            };
                        })
                        .ToList();

                    dashboard.HighestRatingPerEPA = EPAList.ToDictionary(
                        e => e.Id,
                        e => scoredAssessments
                                .Where(s => s.EPAId == e.Id)
                                .Select(s => s.Score)
                                .DefaultIfEmpty(0)
                                .Max()
                    );

                    dashboard.LatestRatingPerEPA = EPAList.ToDictionary(
                        e => e.Id,
                        e => scoredAssessments
                                .Where(s => s.EPAId == e.Id)
                                .OrderByDescending(s => s.Date)
                                .Select(s => s.Score)
                                .FirstOrDefault()
                    );

                    var monthsInTraining = GetMonthsInTraining(user.StartDate);
                    dashboard.MonthsInTraining = monthsInTraining;
                    // Step 1: Match curriculum ≤ months in training
                    var epaCurricula = EPAList
                        .Select(e => new
                        {
                            EPAId = e.Id,
                            Curriculum = e.EPACurricula
                                .Where(c => c.NumberOfMonths <= monthsInTraining)
                                .OrderByDescending(c => c.NumberOfMonths)
                                .FirstOrDefault()
                        })
                        .ToList();

                    // Step 2: Build dictionary with fallback
                    dashboard.ExpectedRatingPerEPA = EPAList.ToDictionary(
                        e => e.Id,
                        e =>
                        {
                            var match = epaCurricula.FirstOrDefault(x => x.EPAId == e.Id);
                            return match?.Curriculum?.EPAScaleOption?.Rank ?? 0;
                        });
                }

                // General stats
                dashboard.NumberOfRequestsMade = (await assessmentRequestRepository.GetTraineePendingRequests(userId))?.Count ?? 0;
                dashboard.NumberOfRequestsDeclined = (await assessmentRequestRepository.GetTraineeDeclinedRequests(userId))?.Count ?? 0;
                dashboard.NumberOfPendingAssessments = (await assessmentRequestRepository.GetTraineePendingAssessments(userId))?.Count ?? 0;
                dashboard.NumberOfCompletedAssessments = (await assessmentRequestRepository.GetTraineeCompletedAssessments(userId))?.Count ?? 0;
            }


            else if (roles.Contains(Role.Assessor.ToStringValue()))
            {
                var requestsMade = await assessmentRequestRepository.GetAssessorPendingRequests(userId);
                dashboard.NumberOfRequestsMade = requestsMade?.Count ?? 0;

                var requestsDeclined = await assessmentRequestRepository.GetAssessorDeclinedRequests(userId);
                dashboard.NumberOfRequestsDeclined = requestsDeclined?.Count ?? 0;

                var requestsMadeAndAccepted = await assessmentRequestRepository.GetAssessorPendingAssessments(userId);
                dashboard.NumberOfPendingAssessments = requestsMadeAndAccepted?.Count ?? 0;

                var completedAssessments = await assessmentRequestRepository.GetAssessorCompletedAssessments(userId);
                dashboard.NumberOfCompletedAssessments = completedAssessments?.Count ?? 0;
            }
            //else if (roles.Contains(Role.Administrator.ToStringValue()))
            //{
            //    dashboard.NumberOfInstitutions = (await institutionRepository.GetAllAsync())?.Count ?? 0;
            //    dashboard.NumberOfSpecialities = (await specialityRepository.GetAllAsync())?.Count ?? 0;
            //    dashboard.NumberOfSubSpecialities = (await subSpecialityRepository.GetAllAsync())?.Count ?? 0;
            //    dashboard.NumberOfAssessmentForms = (await assessmentFormRepository.GetAllAsync())?.Count ?? 0;
            //    dashboard.NumberOfEPAs = (await EPARepository.GetAllAsync())?.Count ?? 0;
            //    dashboard.NumberOfUsers = userManager.Users.Count();
            //    dashboard.NumberOfInvitations = (await registrationInvitationRepository.GetAllAsync())?.Count ?? 0;
            //}

            if (userContext.CanManageUsers)
                dashboard.NumberOfUsers = userManager.Users.Count();

            if (userContext.CanManageInvitations)
                dashboard.NumberOfInvitations = (await registrationInvitationRepository.GetAllAsync())?.Count ?? 0;

            dashboard.NumberOfSpecialities = (await specialityRepository.GetAllAsync())?.Count ?? 0;
            dashboard.NumberOfSubSpecialities = (await subSpecialityRepository.GetAllAsync())?.Count ?? 0;

            if (userContext.CanManageAssessmentForms)
                dashboard.NumberOfAssessmentForms = (await assessmentFormRepository.GetAllAsync())?.Count ?? 0;

            dashboard.NumberOfEPAs = (await EPARepository.GetAllAsync())?.Count ?? 0;

            dashboard.User.Institution = mapper.Map<InstitutionVM>(await institutionRepository.GetAsync(user.InstitutionId));               

            return View(dashboard);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionHandlerPathFeature != null)
            {
                Exception exception = exceptionHandlerPathFeature.Error;
                _logger.LogError(exception, $"Error encountered by user: {this.User?.Identity?.Name} with RequestId: {requestId}");
            }

            return View(new ErrorViewModel { RequestId = requestId });
        }
    }
}
