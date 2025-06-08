using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Contracts;
using Wombat.Common.Constants;
using Wombat.Common.Models;
using Wombat.Data;

namespace Wombat.Controllers
{
    [Authorize(Roles = Roles.Administrator)]
    public class AdminController : Controller
    {
        private readonly UserManager<WombatUser> userManager;
        private readonly IInstitutionRepository institutionRepository;
        private readonly ISpecialityRepository specialityRepository;
        private readonly ISubSpecialityRepository subSpecialityRepository;
        private readonly IAssessmentFormRepository assessmentFormRepository;
        private readonly IEPARepository epaRepository;
        private readonly ILoggedAssessmentRepository loggedAssessmentRepository;

        public AdminController(
            UserManager<WombatUser> userManager,
            IInstitutionRepository institutionRepository,
            ISpecialityRepository specialityRepository,
            ISubSpecialityRepository subSpecialityRepository,
            IAssessmentFormRepository assessmentFormRepository,
            IEPARepository epaRepository,
            ILoggedAssessmentRepository loggedAssessmentRepository)
        {
            this.userManager = userManager;
            this.institutionRepository = institutionRepository;
            this.specialityRepository = specialityRepository;
            this.subSpecialityRepository = subSpecialityRepository;
            this.assessmentFormRepository = assessmentFormRepository;
            this.epaRepository = epaRepository;
            this.loggedAssessmentRepository = loggedAssessmentRepository;
        }

        public async Task<IActionResult> Dashboard()
        {
            var vm = new AdminDashboardVM
            {
                UserCount = await userManager.Users.CountAsync(),
                InstitutionCount = (await institutionRepository.GetAllAsync())?.Count ?? 0,
                SpecialityCount = (await specialityRepository.GetAllAsync())?.Count ?? 0,
                SubSpecialityCount = (await subSpecialityRepository.GetAllAsync())?.Count ?? 0,
                AssessmentFormCount = (await assessmentFormRepository.GetAllAsync())?.Count ?? 0,
                EPACount = (await epaRepository.GetAllAsync())?.Count ?? 0,
                LoggedAssessmentCount = (await loggedAssessmentRepository.GetAllAsync())?.Count ?? 0
            };

            return View(vm);
        }
    }
}
