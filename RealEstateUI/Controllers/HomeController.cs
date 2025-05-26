using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace RealEstateUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration; // Configuration'ı burada tanımlıyoruz.

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration; // Constructor üzerinden gelen configuration'ı burada alıyoruz.
        }

        public IActionResult Index()
        {
            var ApiBaseURL = _configuration["ApiBaseURL"];
            ViewBag.ApiBaseURL = ApiBaseURL;
            return View();
        }

        [Route("Login")]
        public IActionResult Login()
        {
            var ApiBaseURL = _configuration["ApiBaseURL"];
            ViewBag.ApiBaseURL = ApiBaseURL;
            return View();
        }

        [Route("Register")]
        public IActionResult Register()
        {
            ViewBag.ApiBaseURL = _configuration["ApiBaseURL"];
            return View();
        }

        public IActionResult ListingsAll()
        {
            var apiBaseUrl = _configuration["ApiBaseURL"];
            ViewBag.ApiBaseURL = apiBaseUrl;
            return View();
        }

        public IActionResult ListingsSale()
        {
            var apiBaseUrl = _configuration["ApiBaseURL"];
            ViewBag.ApiBaseURL = apiBaseUrl;
            return View();
        }

        public IActionResult ListingsRent()
        {
            var apiBaseUrl = _configuration["ApiBaseURL"];
            ViewBag.ApiBaseURL = apiBaseUrl;
            return View();
        }

        public IActionResult Details(int id)
        {
            var apiBaseUrl = _configuration["ApiBaseURL"];
            ViewBag.ApiBaseURL = apiBaseUrl;
            return View();
        }
        public IActionResult Favorites()
        {
            var apiBaseUrl = _configuration["ApiBaseURL"];
            ViewBag.ApiBaseURL = apiBaseUrl;
            return View();
        }

        public IActionResult Profile()
        {
            var apiBaseUrl = _configuration["ApiBaseURL"];
            ViewBag.ApiBaseURL = apiBaseUrl;
            return View();
        }

    }
}
