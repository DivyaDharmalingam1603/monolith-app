using EnergyLegacyApp.Data;
using EnergyLegacyApp.Data.Models;
using System.Collections.Generic;
using System;


namespace EnergyLegacyApp.Business
{
    // ANTI-PATTERN: God class with too many responsibilities
    public class PowerPlantService
    {
        private readonly PowerPlantRepository _repository;
        private readonly EnergyConsumptionRepository _consumptionRepository;
        
        public PowerPlantService(PowerPlantRepository repository, EnergyConsumptionRepository consumptionRepository)
        {
            _repository = repository;
            _consumptionRepository = consumptionRepository;
        }
        
        // ANTI-PATTERN: No validation, business logic mixed with data access
        public List<PowerPlant> GetAllPowerPlants()
        {
            var plants = _repository.GetAllPowerPlants();
            
            // ANTI-PATTERN: Business logic in service layer that should be in domain
            foreach (var plant in plants)
            {
                if (plant.Type == "Nuclear" && plant.EfficiencyRating < 0.8m)
                {
                    plant.Status = "Needs Review";
                }
            }
            
            return plants;
        }
        
        // ANTI-PATTERN: No error handling, magic numbers
        public PowerPlant? GetPowerPlantById(int id)
        {
            if (id <= 0) return null;
            
            var plant = _repository.GetPowerPlantById(id);
            
            // ANTI-PATTERN: Hardcoded business rules
            if (plant != null && plant.CurrentOutput > plant.Capacity * 0.95m)
            {
                plant.Status = "At Capacity";
            }
            
            return plant;
        }
        
        // ANTI-PATTERN: No input validation
        public bool CreatePowerPlant(PowerPlant plant)
        {
            // ANTI-PATTERN: Minimal validation
            if (string.IsNullOrEmpty(plant.Name))
                return false;
                
            // ANTI-PATTERN: Business logic scattered
            // Note: CarbonEmissions would be tracked separately in EnergyConsumption table
            
            plant.CommissionDate = DateTime.Now;
            plant.LastMaintenanceDate = DateTime.Now;
            
            return _repository.InsertPowerPlant(plant);
        }
        
        // ANTI-PATTERN: Complex business logic in service layer
        public bool UpdatePowerPlant(PowerPlant plant)
        {
            var existingPlant = _repository.GetPowerPlantById(plant.Id);
            if (existingPlant == null) return false;
            
            // ANTI-PATTERN: Hardcoded business rules
            if (plant.Type != existingPlant.Type)
            {
                // Should validate type change is allowed
                if (existingPlant.Type == "Nuclear" && plant.Type != "Nuclear")
                {
                    // Nuclear plants can't change type easily
                    return false;
                }
            }
            
            // ANTI-PATTERN: No audit trail
            return _repository.UpdatePowerPlant(plant);
        }
        
        // ANTI-PATTERN: No soft delete, no cascade delete consideration
        public bool DeletePowerPlant(int id)
        {
            // ANTI-PATTERN: No check for related data
            return _repository.DeletePowerPlant(id);
        }
        
        // ANTI-PATTERN: Inefficient data processing
        public List<PowerPlant> GetPowerPlantsByEfficiency(decimal minEfficiency)
        {
            var allPlants = _repository.GetAllPowerPlants();
            var efficientPlants = new List<PowerPlant>();
            
            // ANTI-PATTERN: Filtering in application instead of database
            foreach (var plant in allPlants)
            {
                if (plant.EfficiencyRating >= minEfficiency)
                {
                    efficientPlants.Add(plant);
                }
            }
            
            return efficientPlants;
        }
        
        // ANTI-PATTERN: Complex calculations in service layer
        public decimal CalculateTotalCapacity(string region)
        {
            var allPlants = _repository.GetAllPowerPlants();
            decimal totalCapacity = 0;
            
            foreach (var plant in allPlants)
            {
                if (plant.Location.Contains(region)) // ANTI-PATTERN: String contains for region matching
                {
                    totalCapacity += plant.Capacity;
                }
            }
            
            return totalCapacity;
        }
        
        // ANTI-PATTERN: Mixing different concerns in one method
        public string GeneratePowerPlantReport(int plantId)
        {
            var plant = _repository.GetPowerPlantById(plantId);
            if (plant == null) return "Plant not found";
            
            var consumption = _consumptionRepository.GetConsumptionByPowerPlant(plantId);
            
            // ANTI-PATTERN: String concatenation for report generation
            var report = "Power Plant Report\n";
            report += "==================\n";
            report += $"Name: {plant.Name}\n";
            report += $"Type: {plant.Type}\n";
            report += $"Capacity: {plant.Capacity} MW\n";
            report += $"Current Output: {plant.CurrentOutput} MW\n";
            report += $"Efficiency: {plant.EfficiencyRating * 100}%\n";
            report += $"Status: {plant.Status}\n";
            report += $"Total Consumption Records: {consumption.Count}\n";
            
            // ANTI-PATTERN: Business logic for formatting
            if (plant.EfficiencyRating < 0.7m)
            {
                report += "WARNING: Low efficiency rating!\n";
            }
            
            return report;
        }
    }
}