using System.Diagnostics;
using ASPLeafMapInsightsWebMVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace ASPLeafMapInsightsWebMVC.Controllers
{
    /// <summary>Начална страница, Privacy и обща страница за грешки. Не използва API.</summary>
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

        /// <summary>Показва се при необработено изключение (в Production чрез UseExceptionHandler).</summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
