using EnergyLegacyApp.Business;
using EnergyLegacyApp.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EnergyLegacyApp.Web.Controllers
{
    [Authorize]
    public class PowerPlantController : Controller
    {
        private readonly PowerPlantService _powerPlantService;
        private readonly EnergyAnalyticsService _energyAnalyticsService;
        private readonly ILogger<PowerPlantController> _logger;

        public PowerPlantController(PowerPlantService powerPlantService, ILogger<PowerPlantController> logger, EnergyAnalyticsService energyAnalyticsService)
        {
            _powerPlantService = powerPlantService;
            _logger = logger;
            _energyAnalyticsService = energyAnalyticsService;

        }

        public IActionResult Index()
        {
            var plants = _powerPlantService.GetAllPowerPlants();
            return View(plants);
        }

        public IActionResult Details(int id)
        {
            var plant = _powerPlantService.GetPowerPlantById(id);

            if (plant == null)
                return NotFound();

            ViewBag.Report = _powerPlantService.GeneratePowerPlantReport(id);
            return View(plant);
        }

        public IActionResult Create()
        {
            ViewBag.PlantTypes = new List<string> { "Coal", "Gas", "Nuclear", "Solar", "Wind", "Hydro" };
            ViewBag.StatusOptions = new List<string> { "Active", "Maintenance", "Decommissioned" };
            return View();
        }

        [HttpPost]
        public IActionResult Create(PowerPlant plant)
        {
            try
            {
                var success = _powerPlantService.CreatePowerPlant(plant);

                if (success)
                {
                    _logger.LogInformation("Power plant '{Name}' created by user.", plant.Name);
                    TempData["Success"] = "Power plant created successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    _logger.LogWarning("Failed to create power plant '{Name}'.", plant.Name);
                    TempData["Error"] = "Failed to create power plant";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while creating power plant '{Name}'.", plant.Name);
                TempData["Error"] = $"Error: {ex.Message}";
            }

            ViewBag.PlantTypes = new List<string> { "Coal", "Gas", "Nuclear", "Solar", "Wind", "Hydro" };
            ViewBag.StatusOptions = new List<string> { "Active", "Maintenance", "Decommissioned" };
            return View(plant);
        }

        public IActionResult Edit(int id)
        {
            var plant = _powerPlantService.GetPowerPlantById(id);
            if (plant == null)
                return NotFound();

            ViewBag.PlantTypes = new List<string> { "Coal", "Gas", "Nuclear", "Solar", "Wind", "Hydro" };
            ViewBag.StatusOptions = new List<string> { "Active", "Maintenance", "Decommissioned" };
            return View(plant);
        }

        [HttpPost]
        public IActionResult Edit(PowerPlant plant)
        {
            try
            {
                var success = _powerPlantService.UpdatePowerPlant(plant);

                if (success)
                {
                    _logger.LogInformation("Power plant '{Name}' updated successfully.", plant.Name);
                    TempData["Success"] = "Power plant updated successfully!";
                    return RedirectToAction("Details", new { id = plant.Id });
                }
                else
                {
                    _logger.LogWarning("Failed to update power plant '{Name}'.", plant.Name);
                    TempData["Error"] = "Failed to update power plant";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while updating power plant '{Name}'.", plant.Name);
                TempData["Error"] = $"Error: {ex.Message}";
            }

            ViewBag.PlantTypes = new List<string> { "Coal", "Gas", "Nuclear", "Solar", "Wind", "Hydro" };
            ViewBag.StatusOptions = new List<string> { "Active", "Maintenance", "Decommissioned" };
            return View(plant);
        }

        public IActionResult Delete(int id)
        {
            try
            {
                var plant = _powerPlantService.GetPowerPlantById(id);
                var success = _powerPlantService.DeletePowerPlant(id);

                if (success)
                {
                    _logger.LogInformation("Power plant with ID {Id} ('{Name}') deleted successfully.", id, plant?.Name ?? "Unknown");
                    TempData["Success"] = "Power plant deleted successfully!";
                }
                else
                {
                    _logger.LogWarning("Failed to delete power plant with ID {Id}.", id);
                    TempData["Error"] = "Failed to delete power plant";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while deleting power plant with ID {Id}.", id);
                TempData["Error"] = $"Error: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        public IActionResult ExportCsv()
        {
            try
            {
                var csvData = _energyAnalyticsService.ExportDataToCsv("plants");

                var bytes = System.Text.Encoding.UTF8.GetBytes(csvData);
                return File(bytes, "text/csv", "powerplants.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Export CSV failed.");
                TempData["Error"] = $"Export failed: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}
