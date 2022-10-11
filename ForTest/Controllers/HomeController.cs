using ForTest.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ForTest.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(ILogger<HomeController> logger,IWebHostEnvironment environment)
        {
            _logger = logger;
            _webHostEnvironment = environment;
        }

        public IActionResult Index()
        {
            ViewBag.Environment = _webHostEnvironment.EnvironmentName;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}