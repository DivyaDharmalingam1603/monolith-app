using System;

namespace EnergyLegacyApp.Data.Models
{
    // ANTI-PATTERN: Anemic domain model with no behavior
    public class PowerPlant
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Coal, Gas, Nuclear, Solar, Wind
        public decimal Capacity { get; set; } // MW
        public string Location { get; set; } = string.Empty;
        public DateTime CommissionDate { get; set; }
        public string Status { get; set; } = string.Empty; // Active, Maintenance, Decommissioned
        public decimal CurrentOutput { get; set; } // Current MW output
        public decimal EfficiencyRating { get; set; }
        public string OperatorCompany { get; set; } = string.Empty;
        public decimal MaintenanceCost { get; set; }
        public DateTime LastMaintenanceDate { get; set; }
    }
}