using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
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

        public IActionResult About()
        {
            ViewData["Message"] = "WWWWWWWWWWWWWWWW";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CalculateAge(UserModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CurrentYear = DateTime.Now.Year;
                return View("Index", model);
            }
            int age = DateTime.Now.Year - model.BirthYear;

            var result = new AgeResultModel
            {
                UserName = model.Name,
                Age = age,
            };
            HttpContext.Session.SetString("UserName", model.Name);
            HttpContext.Session.SetInt32("UserAge", age);
            return View("AgeResult ", result);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
