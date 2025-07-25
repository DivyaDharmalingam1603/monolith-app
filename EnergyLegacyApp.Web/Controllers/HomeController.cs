using EnergyLegacyApp.Business;
using EnergyLegacyApp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace EnergyLegacyApp.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EnergyAnalyticsService _analyticsService;
        
        public HomeController(EnergyAnalyticsService analyticsService, ILogger<HomeController> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
            _logger.LogInformation("Home Controller Constructor");
        }

        // ANTI-PATTERN: Controller doing business logic
        public IActionResult Index()
        {
            _logger.LogInformation("Hello from......Home Page");
            try
            {
                var dashboardData = _analyticsService.GetDashboardData();
                
                // ANTI-PATTERN: ViewBag instead of strongly typed models
                ViewBag.TotalCapacity = dashboardData["TotalCapacity"];
                ViewBag.ActivePlants = dashboardData["ActivePlants"];
                ViewBag.TotalPlants = dashboardData["TotalPlants"];
                ViewBag.MonthlyConsumption = dashboardData["MonthlyConsumption"];
                ViewBag.MonthlyEmissions = dashboardData["MonthlyEmissions"];
                ViewBag.PlantsByType = dashboardData["PlantsByType"];
                
                var alerts = _analyticsService.GetMaintenanceAlerts();
                ViewBag.MaintenanceAlerts = alerts;
                
                return View();
            }
            catch (Exception ex)
            {
                // ANTI-PATTERN: Catching all exceptions and showing generic error
                ViewBag.Error = "Unable to load dashboard data";
                return View();
            }
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
