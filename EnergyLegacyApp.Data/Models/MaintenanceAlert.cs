namespace EnergyLegacyApp.Data.Models
{
    public class MaintenanceAlert
    {
        public int PlantId { get; set; }
        public string PlantName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int DaysOverdue { get; set; }
        public string Priority { get; set; } = string.Empty;
    }
}
