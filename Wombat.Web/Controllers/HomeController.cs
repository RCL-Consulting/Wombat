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
using System.Diagnostics;
using Wombat.Application.Contracts;
using Wombat.Application.Repositories;
using Wombat.Common.Constants;
using Wombat.Common.Models;
using Wombat.Data;

namespace Wombat.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<WombatUser> userManager;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ISubSpecialityRepository subSpecialityRepository;
        private readonly IInstitutionRepository institutionRepository;
        private readonly IEPARepository EPARepository;
        private readonly IAssessmentRequestRepository assessmentRequestRepository;
        private readonly ILoggedAssessmentRepository loggedAssessmentRepository;
        private readonly IMapper mapper;

        public HomeController( UserManager<WombatUser> userManager,
                               IHttpContextAccessor httpContextAccessor,
                               ISubSpecialityRepository subSpecialityRepository,
                               ILogger<HomeController> logger,
                               IMapper mapper,
                               IInstitutionRepository institutionRepository,
                               IEPARepository EPARepository,
                               IAssessmentRequestRepository assessmentRequestRepository,
                               ILoggedAssessmentRepository loggedAssessmentRepository )
        {
            this.userManager = userManager;
            this.httpContextAccessor = httpContextAccessor;
            this.subSpecialityRepository = subSpecialityRepository;
            this.mapper = mapper;
            _logger = logger;
            this.institutionRepository = institutionRepository;
            this.EPARepository = EPARepository;
            this.assessmentRequestRepository = assessmentRequestRepository;
            this.loggedAssessmentRepository = loggedAssessmentRepository;
        }

        public async Task<IActionResult> IndexAsync()
        {
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

            if (roles.Contains(Roles.Trainee))
            {
                dashboard.User.SubSpeciality = mapper.Map<SubSpecialityVM>(await subSpecialityRepository.GetAsync(user.SubSpecialityId));
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
                }

                var requestsMade = await assessmentRequestRepository.GetTraineePendingRequests(userId);
                dashboard.NumberOfRequestsMade = requestsMade?.Count ?? 0;

                var requestsDeclined = await assessmentRequestRepository.GetTraineeDeclinedRequests(userId);
                dashboard.NumberOfRequestsDeclined = requestsDeclined?.Count ?? 0;

                var pendingAssessments = await assessmentRequestRepository.GetTraineePendingAssessments(userId);
                dashboard.NumberOfPendingAssessments = pendingAssessments?.Count ?? 0;

                var completedAssessments = await assessmentRequestRepository.GetTraineeCompletedAssessments(userId);
                dashboard.NumberOfCompletedAssessments = completedAssessments?.Count ?? 0;
            }
            else if (roles.Contains(Roles.Assessor))
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
