using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Wombat.Common.Constants;
using Wombat.Common.Models;

namespace Wombat.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
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
