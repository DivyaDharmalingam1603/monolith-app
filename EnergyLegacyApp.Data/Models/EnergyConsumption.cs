using System;

namespace EnergyLegacyApp.Data.Models
{
    public class EnergyConsumption
    {
        public int Id { get; set; }
        public int PowerPlantId { get; set; }
        public DateTime RecordDate { get; set; }
        public decimal ConsumptionMWh { get; set; }
        public decimal PeakDemand { get; set; }
        public string Region { get; set; } = string.Empty;
        public decimal CostPerMWh { get; set; }
        public string ConsumerType { get; set; } = string.Empty; // Residential, Commercial, Industrial
        public decimal CarbonEmissions { get; set; } // CO2 tons
    }
}