using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Wombat.Application.Contracts;
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
        private readonly IMapper mapper;

        public HomeController( UserManager<WombatUser> userManager,
                               IHttpContextAccessor httpContextAccessor,
                               ISubSpecialityRepository subSpecialityRepository,
                               ILogger<HomeController> logger,
                               IMapper mapper,
                               IInstitutionRepository institutionRepository,
                               IEPARepository EPARepository )
        {
            this.userManager = userManager;
            this.httpContextAccessor = httpContextAccessor;
            this.subSpecialityRepository = subSpecialityRepository;
            this.mapper = mapper;
            _logger = logger;
            this.institutionRepository = institutionRepository;
            this.EPARepository = EPARepository;
        }

        public async Task<IActionResult> IndexAsync()
        {
            if (httpContextAccessor.HttpContext == null)
                return NotFound();

            var userId = userManager.GetUserId(httpContextAccessor.HttpContext.User);
            var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext.User);

            var dashboard = new DashboardVM
            {
                User = mapper.Map<WombatUserVM>(user),
                EPAList = new List<EPAVM>()
            };

            dashboard.User.SubSpeciality = mapper.Map<SubSpecialityVM>(await subSpecialityRepository.GetAsync(user.SubSpecialityId));
            if (dashboard.User.SubSpeciality == null)
                return NotFound();
            dashboard.User.Speciality = dashboard.User.SubSpeciality.Speciality;
            dashboard.User.Institution = mapper.Map<InstitutionVM>(await institutionRepository.GetAsync(user.InstitutionId));

            dashboard.EPAList = mapper.Map<List<EPAVM>>(await EPARepository.GetEPAListBySubspeciality(dashboard.User.SubSpeciality.Id));

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
