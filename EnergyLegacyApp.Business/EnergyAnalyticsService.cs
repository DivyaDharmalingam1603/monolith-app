using EnergyLegacyApp.Data;
using EnergyLegacyApp.Data.Models;
using System;
using System.Collections.Generic;

namespace EnergyLegacyApp.Business
{
    // ANTI-PATTERN: Another god class with mixed responsibilities
    public class EnergyAnalyticsService
    {
        private readonly EnergyConsumptionRepository _consumptionRepository;
        private readonly PowerPlantRepository _powerPlantRepository;

        // ✅ Inject both repositories via constructor
        public EnergyAnalyticsService(EnergyConsumptionRepository consumptionRepository, PowerPlantRepository powerPlantRepository)
        {
            _consumptionRepository = consumptionRepository;
            _powerPlantRepository = powerPlantRepository;
        }

        // ANTI-PATTERN: Inefficient data processing, loading all data
        public Dictionary<string, object> GetDashboardData()
        {
            var dashboardData = new Dictionary<string, object>();
            
            // ANTI-PATTERN: Multiple database calls that could be optimized
            var allPlants = _powerPlantRepository.GetAllPowerPlants();
            decimal totalCapacity = 0m;
            var activePlants = 0;
            var plantsByType = new Dictionary<string, int>();
            
            foreach (var plant in allPlants)
            {
                totalCapacity += plant.Capacity;
                if (plant.Status == "Active")
                    activePlants++;
                    
                if (plantsByType.ContainsKey(plant.Type))
                    plantsByType[plant.Type]++;
                else
                    plantsByType[plant.Type] = 1;
            }
            
            // ANTI-PATTERN: Hardcoded date ranges
            var lastMonth = DateTime.Now.AddDays(-30);
            var consumptionData = _consumptionRepository.GetConsumptionByDateRange(lastMonth, DateTime.Now);
            
            decimal totalConsumption = 0m;
            decimal totalEmissions = 0m;
            
            foreach (var consumption in consumptionData)
            {
                totalConsumption += consumption.ConsumptionMWh;
                totalEmissions += consumption.CarbonEmissions;
            }
            
            dashboardData["TotalCapacity"] = totalCapacity;
            dashboardData["ActivePlants"] = activePlants;
            dashboardData["TotalPlants"] = allPlants.Count;
            dashboardData["PlantsByType"] = plantsByType;
            dashboardData["MonthlyConsumption"] = totalConsumption;
            dashboardData["MonthlyEmissions"] = totalEmissions;
            
            return dashboardData;
        }
        
        // ANTI-PATTERN: Complex business logic without proper abstraction
        public List<object> GetEfficiencyAnalysis()
        {
            var analysis = new List<object>();
            var plants = _powerPlantRepository.GetAllPowerPlants();
            
            foreach (var plant in plants)
            {
                var efficiency = plant.EfficiencyRating;
                var status = "Good";
                
                // ANTI-PATTERN: Magic numbers and hardcoded thresholds
                if (efficiency < 0.6m)
                    status = "Poor";
                else if (efficiency < 0.8m)
                    status = "Average";
                else if (efficiency > 0.95m)
                    status = "Excellent";
                
                // ANTI-PATTERN: Anonymous objects making it hard to maintain
                analysis.Add(new
                {
                    PlantName = plant.Name,
                    Type = plant.Type,
                    Efficiency = efficiency,
                    Status = status,
                    Recommendation = GetEfficiencyRecommendation(plant)
                });
            }
            
            return analysis;
        }
        
        // ANTI-PATTERN: Hardcoded business rules
        private string GetEfficiencyRecommendation(PowerPlant plant)
        {
            if (plant.EfficiencyRating < 0.6m)
            {
                return "Consider major overhaul or replacement";
            }
            else if (plant.EfficiencyRating < 0.8m)
            {
                return "Schedule maintenance and efficiency upgrades";
            }
            else if (plant.Type == "Coal" && plant.EfficiencyRating < 0.85m)
            {
                return "Consider conversion to cleaner fuel source";
            }
            
            return "Operating at optimal efficiency";
        }
        
        // ANTI-PATTERN: No caching, expensive calculations every time
        public Dictionary<string, decimal> GetRegionalAnalysis()
        {
            var regionalData = new Dictionary<string, decimal>();
            var consumptionSummary = _consumptionRepository.GetConsumptionByRegionSummary();
            
            // ANTI-PATTERN: Hardcoded region list
            var regions = new[] { "North", "South", "East", "West", "Central" };
            
            foreach (var region in regions)
            {
                if (consumptionSummary.ContainsKey(region))
                {
                    regionalData[region] = consumptionSummary[region];
                }
                else
                {
                    regionalData[region] = 0;
                }
            }
            
            return regionalData;
        }
        
        // ANTI-PATTERN: Synchronous operations that could be async
        public List<MaintenanceAlert> GetMaintenanceAlerts()
        {
            var alerts = new List<MaintenanceAlert>();
            var plants = _powerPlantRepository.GetAllPowerPlants();
            
            foreach (var plant in plants)
            {
                var daysSinceLastMaintenance = (DateTime.Now - plant.LastMaintenanceDate).Days;
                
                // ANTI-PATTERN: Hardcoded maintenance schedules
                var maintenanceInterval = plant.Type switch
                {
                    "Nuclear" => 90,
                    "Coal" => 120,
                    "Gas" => 180,
                    "Solar" => 365,
                    "Wind" => 180,
                    _ => 365
                };
                
                if (daysSinceLastMaintenance > maintenanceInterval)
                {
                    alerts.Add(new MaintenanceAlert
                    {
                        PlantId = plant.Id,
                        PlantName = plant.Name,
                        Type = plant.Type,
                        DaysOverdue = daysSinceLastMaintenance - maintenanceInterval,
                        Priority = daysSinceLastMaintenance > maintenanceInterval * 1.5m ? "High" : "Medium"
                    });
                }
            }
            
            return alerts;
        }
        
        // ANTI-PATTERN: String manipulation for data export
        public string ExportDataToCsv(string dataType)
        {
            var csv = "";
            
            if (dataType == "plants")
            {
                var plants = _powerPlantRepository.GetAllPowerPlants();
                csv = "Id,Name,Type,Capacity,Location,Status,CurrentOutput,Efficiency\n";
                
                foreach (var plant in plants)
                {
                    csv += $"{plant.Id},{plant.Name},{plant.Type},{plant.Capacity},{plant.Location},{plant.Status},{plant.CurrentOutput},{plant.EfficiencyRating}\n";
                }
            }
            else if (dataType == "consumption")
            {
                var lastMonth = DateTime.Now.AddDays(-30);
                var consumption = _consumptionRepository.GetConsumptionByDateRange(lastMonth, DateTime.Now);
                csv = "Id,PowerPlantId,Date,ConsumptionMWh,PeakDemand,Region,CostPerMWh\n";
                
                foreach (var record in consumption)
                {
                    csv += $"{record.Id},{record.PowerPlantId},{record.RecordDate:yyyy-MM-dd},{record.ConsumptionMWh},{record.PeakDemand},{record.Region},{record.CostPerMWh}\n";
                }
            }
            
            return csv;
        }
    }
}