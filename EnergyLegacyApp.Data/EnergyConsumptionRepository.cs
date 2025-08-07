using EnergyLegacyApp.Data.Models;
using System.Data;
namespace EnergyLegacyApp.Data
{
    public class EnergyConsumptionRepository
    {
        private readonly DatabaseHelper _db;

        // ✅ Inject DatabaseHelper via constructor
        public EnergyConsumptionRepository(DatabaseHelper dbHelper)
        {
            _db = dbHelper;
        }

        public List<EnergyConsumption> GetConsumptionByDateRange(DateTime startDate, DateTime endDate)
        {
            var consumptions = new List<EnergyConsumption>();
            var query = $@"SELECT 
                Id,
                PowerPlantId,
                RecordDate,
                ConsumptionMWh,
                PeakDemand,
                Region,
                CostPerMWh,
                ConsumerType,
                CarbonEmissions
            FROM EnergyConsumption 
            WHERE RecordDate >= '{startDate:yyyy-MM-dd}' 
              AND RecordDate <= '{endDate:yyyy-MM-dd}' 
            ORDER BY RecordDate DESC";

            var dataTable = _db.ExecuteQuery(query); // ✅ use injected instance

            foreach (DataRow row in dataTable.Rows)
            {
                consumptions.Add(MapRowToEnergyConsumption(row));
            }

            return consumptions;
        }

        public List<EnergyConsumption> GetConsumptionByPowerPlant(int powerPlantId)
        {
            var consumptions = new List<EnergyConsumption>();
            var query = $@"SELECT 
                Id,
                PowerPlantId,
                RecordDate,
                ConsumptionMWh,
                PeakDemand,
                Region,
                CostPerMWh,
                ConsumerType,
                CarbonEmissions
            FROM EnergyConsumption WHERE PowerPlantId = {powerPlantId}";

            var dataTable = _db.ExecuteQuery(query);

            foreach (DataRow row in dataTable.Rows)
            {
                consumptions.Add(MapRowToEnergyConsumption(row));
            }

            return consumptions;
        }

        public bool InsertConsumptionRecord(EnergyConsumption consumption)
        {
            var query = $@"INSERT INTO EnergyConsumption 
                (PowerPlantId, RecordDate, ConsumptionMWh, PeakDemand, Region, CostPerMWh, ConsumerType, CarbonEmissions) 
                VALUES 
                ({consumption.PowerPlantId}, '{consumption.RecordDate:yyyy-MM-dd HH:mm:ss}', 
                {consumption.ConsumptionMWh.ToString(System.Globalization.CultureInfo.InvariantCulture)}, 
                {consumption.PeakDemand.ToString(System.Globalization.CultureInfo.InvariantCulture)}, 
                '{consumption.Region}', 
                {consumption.CostPerMWh.ToString(System.Globalization.CultureInfo.InvariantCulture)}, 
                '{consumption.ConsumerType}', 
                {consumption.CarbonEmissions.ToString(System.Globalization.CultureInfo.InvariantCulture)})";

            var result = _db.ExecuteNonQuery(query);
            return result > 0;
        }

        public decimal GetTotalConsumptionByRegion(string region)
        {
            var query = $@"SELECT 
                Id,
                PowerPlantId,
                RecordDate,
                ConsumptionMWh,
                PeakDemand,
                Region,
                CostPerMWh,
                ConsumerType,
                CarbonEmissions
            FROM EnergyConsumption WHERE Region = '{region}'";

            var dataTable = _db.ExecuteQuery(query);

            decimal total = 0;
            foreach (DataRow row in dataTable.Rows)
            {
                total += Convert.ToDecimal(row["ConsumptionMWh"]);
            }

            return total;
        }

        public Dictionary<string, decimal> GetConsumptionByRegionSummary()
        {
            var summary = new Dictionary<string, decimal>();
            var query = @"SELECT 
                Id,
                PowerPlantId,
                RecordDate,
                ConsumptionMWh,
                PeakDemand,
                Region,
                CostPerMWh,
                ConsumerType,
                CarbonEmissions
            FROM EnergyConsumption";

            var dataTable = _db.ExecuteQuery(query);

            foreach (DataRow row in dataTable.Rows)
            {
                var region = row["Region"].ToString();
                var consumption = Convert.ToDecimal(row["ConsumptionMWh"]);

                if (region != null && summary.ContainsKey(region))
                {
                    summary[region] += consumption;
                }
                else if (region != null)
                {
                    summary[region] = consumption;
                }
            }

            return summary;
        }
        private EnergyConsumption MapRowToEnergyConsumption(DataRow row)
        {
            return new EnergyConsumption
            {
                Id = Convert.ToInt32(row["Id"]),
                PowerPlantId = Convert.ToInt32(row["PowerPlantId"]),
                RecordDate = Convert.ToDateTime(row["RecordDate"]),
                ConsumptionMWh = Convert.ToDecimal(row["ConsumptionMWh"]),
                PeakDemand = Convert.ToDecimal(row["PeakDemand"]),
                Region = row["Region"].ToString() ?? string.Empty,
                CostPerMWh = Convert.ToDecimal(row["CostPerMWh"]),
                ConsumerType = row["ConsumerType"].ToString() ?? string.Empty,
                CarbonEmissions = Convert.ToDecimal(row["CarbonEmissions"]),
            };
        }
    }
}
