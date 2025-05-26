using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace RealEstateUI.Controllers
{
    public class AdminController : Controller
    {
        private readonly IConfiguration _configuration;

        public AdminController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var ApiBaseURL = _configuration["ApiBaseURL"];
            ViewBag.ApiBaseURL = ApiBaseURL;
            return View();
        }

        [Route("Admin/Categories")]
        public IActionResult Categories()
        {
            var ApiBaseURL = _configuration["ApiBaseURL"];
            ViewBag.ApiBaseURL = ApiBaseURL;
            return View();
        }

        [Route("Admin/Listings")]
        public IActionResult Listings()
        {
            var ApiBaseURL = _configuration["ApiBaseURL"];
            ViewBag.ApiBaseURL = ApiBaseURL;
            return View();
        }

        [Route("Admin/Users")]
        public IActionResult Users()
        {
            var apiBaseUrl = _configuration["ApiBaseURL"];
            ViewBag.ApiBaseURL = apiBaseUrl;
            return View();
        }
    }
}
