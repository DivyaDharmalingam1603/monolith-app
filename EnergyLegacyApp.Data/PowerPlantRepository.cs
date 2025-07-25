using EnergyLegacyApp.Data.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace EnergyLegacyApp.Data
{
    // ANTI-PATTERN: No interface, tightly coupled to concrete implementation
    public class PowerPlantRepository
    {
        private readonly DatabaseHelper _db;

        public PowerPlantRepository(DatabaseHelper dbHelper)
        {
            _db = dbHelper;
        }

        // ANTI-PATTERN: SQL injection vulnerabilities throughout
        public List<PowerPlant> GetAllPowerPlants()
        {
            var plants = new List<PowerPlant>();
            var query = @"SELECT 
        Id,
        Name,
        Type,
        Capacity,
        Location,
        CommissionDate,
        Status,
        CurrentOutput,
        EfficiencyRating,
        OperatorCompany,
        MaintenanceCost,
        LastMaintenanceDate
    FROM PowerPlants ORDER BY Name";
            var dataTable = _db.ExecuteQuery(query);
            
            foreach (DataRow row in dataTable.Rows)
            {
                plants.Add(MapRowToPowerPlant(row));
            }
            return plants;
        }
        
        // ANTI-PATTERN: String concatenation for SQL queries
        public PowerPlant GetPowerPlantById(int id)
        {
            var query = $@"SELECT 
        Id,
        Name,
        Type,
        Capacity,
        Location,
        CommissionDate,
        Status,
        CurrentOutput,
        EfficiencyRating,
        OperatorCompany,
        MaintenanceCost,
        LastMaintenanceDate
    FROM PowerPlants WHERE Id = {id}";
            var dataTable = _db.ExecuteQuery(query);
            
            if (dataTable.Rows.Count > 0)
            {
                return MapRowToPowerPlant(dataTable.Rows[0]);
            }
            return null;
        }
        
        // ANTI-PATTERN: No validation, direct string interpolation
        public List<PowerPlant> GetPowerPlantsByType(string type)
        {
            var plants = new List<PowerPlant>();
            var query = $@"SELECT 
        Id,
        Name,
        Type,
        Capacity,
        Location,
        CommissionDate,
        Status,
        CurrentOutput,
        EfficiencyRating,
        OperatorCompany,
        MaintenanceCost,
        LastMaintenanceDate
    FROM PowerPlants WHERE Type = '{type}'";
            var dataTable = _db.ExecuteQuery(query);
            
            foreach (DataRow row in dataTable.Rows)
            {
                plants.Add(MapRowToPowerPlant(row));
            }
            return plants;
        }
        
        // ANTI-PATTERN: No transaction management
        public bool InsertPowerPlant(PowerPlant plant)
        {
            var query = $@"INSERT INTO PowerPlants 
                (Name, Type, Capacity, Location, CommissionDate, Status, CurrentOutput, EfficiencyRating, OperatorCompany, MaintenanceCost, LastMaintenanceDate) 
                VALUES 
                ('{plant.Name}', '{plant.Type}', {plant.Capacity.ToString(System.Globalization.CultureInfo.InvariantCulture)}, '{plant.Location}', 
                '{plant.CommissionDate:yyyy-MM-dd}', '{plant.Status}', {plant.CurrentOutput.ToString(System.Globalization.CultureInfo.InvariantCulture)}, 
                {plant.EfficiencyRating.ToString(System.Globalization.CultureInfo.InvariantCulture)}, '{plant.OperatorCompany}', {plant.MaintenanceCost.ToString(System.Globalization.CultureInfo.InvariantCulture)}, 
                '{plant.LastMaintenanceDate:yyyy-MM-dd}')";
            
            var result = _db.ExecuteNonQuery(query);
            return result > 0;
        }
        
        // ANTI-PATTERN: No error handling
        public bool UpdatePowerPlant(PowerPlant plant)
        {
            var query = $@"UPDATE PowerPlants SET 
                Name = '{plant.Name}',
                Type = '{plant.Type}',
                Capacity = {plant.Capacity.ToString(System.Globalization.CultureInfo.InvariantCulture)},
                Location = '{plant.Location}',
                Status = '{plant.Status}',
                CurrentOutput = {plant.CurrentOutput.ToString(System.Globalization.CultureInfo.InvariantCulture)},
                EfficiencyRating = {plant.EfficiencyRating.ToString(System.Globalization.CultureInfo.InvariantCulture)},
                OperatorCompany = '{plant.OperatorCompany}',
                MaintenanceCost = {plant.MaintenanceCost.ToString(System.Globalization.CultureInfo.InvariantCulture)},
                LastMaintenanceDate = '{plant.LastMaintenanceDate:yyyy-MM-dd}'
                WHERE Id = {plant.Id}";
            
            var result = _db.ExecuteNonQuery(query);
            return result > 0;
        }
        
        // ANTI-PATTERN: Hard delete without soft delete option
        public bool DeletePowerPlant(int id)
        {
            var query = $"DELETE FROM PowerPlants WHERE Id = {id}";
            var result = _db.ExecuteNonQuery(query);
            return result > 0;
        }
        
        // ANTI-PATTERN: No null checking, potential casting issues
        private PowerPlant MapRowToPowerPlant(DataRow row)
        {
            return new PowerPlant
            {
                Id = Convert.ToInt32(row["Id"]),
                Name = row["Name"].ToString(),
                Type = row["Type"].ToString(),
                Capacity = Convert.ToDecimal(row["Capacity"]),
                Location = row["Location"].ToString(),
                CommissionDate = Convert.ToDateTime(row["CommissionDate"]),
                Status = row["Status"].ToString(),
                CurrentOutput = Convert.ToDecimal(row["CurrentOutput"]),
                EfficiencyRating = Convert.ToDecimal(row["EfficiencyRating"]),
                OperatorCompany = row["OperatorCompany"].ToString(),
                MaintenanceCost = Convert.ToDecimal(row["MaintenanceCost"]),
                LastMaintenanceDate = Convert.ToDateTime(row["LastMaintenanceDate"])
            };
        }
    }
}