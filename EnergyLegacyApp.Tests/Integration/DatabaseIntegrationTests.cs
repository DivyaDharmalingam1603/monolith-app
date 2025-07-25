using Microsoft.VisualStudio.TestTools.UnitTesting;
using EnergyLegacyApp.Data;
using EnergyLegacyApp.Data.Models;
using Microsoft.Extensions.Configuration;

namespace EnergyLegacyApp.Tests.Integration
{
    [TestClass]
    public class DatabaseIntegrationTests
    {
        private DatabaseHelper _dbHelper;
        
        [TestInitialize]
        public void Setup()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("TestSettings.json")
                .Build();
            _dbHelper = new DatabaseHelper(config);
        }

        [TestMethod]
        public void DatabaseConnection_ShouldBeAvailable()
        {
            try
            {
                var connection = _dbHelper.GetConnection();
                Assert.IsNotNull(connection);
                Assert.AreEqual(System.Data.ConnectionState.Open, connection.State);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Database connection failed: {ex.Message}");
            }
        }

        [TestMethod]
        public void PowerPlantsTable_ShouldExist()
        {
            try
            {
                var query = "SELECT COUNT(*) FROM PowerPlants";
                var result = _dbHelper.ExecuteScalar(query);
                
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(long));
            }
            catch (Exception ex)
            {
                Assert.Fail($"PowerPlants table test failed: {ex.Message}");
            }
        }

        [TestMethod]
        public void EnergyConsumptionTable_ShouldExist()
        {
            try
            {
                var query = "SELECT COUNT(*) FROM EnergyConsumption";
                var result = _dbHelper.ExecuteScalar(query);
                
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(long));
            }
            catch (Exception ex)
            {
                Assert.Fail($"EnergyConsumption table test failed: {ex.Message}");
            }
        }

        [TestMethod]
        public void SampleData_ShouldExist()
        {
            try
            {
                // ANTI-PATTERN: Testing specific sample data
                var repository = new PowerPlantRepository(_dbHelper);
                var plants = repository.GetAllPowerPlants();
                
                Assert.IsNotNull(plants);
                Assert.IsTrue(plants.Count > 0, "No sample data found");
                
                // ANTI-PATTERN: Testing hardcoded sample data
                var solarPlants = plants.Where(p => p.Type == "Solar").ToList();
                Assert.IsTrue(solarPlants.Count > 0, "No solar plants found in sample data");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Sample data test failed: {ex.Message}");
            }
        }

        [TestMethod]
        public void ForeignKeyConstraints_ShouldWork()
        {
            try
            {
                // ANTI-PATTERN: Testing database constraints through application code
                var consumptionRepo = new EnergyConsumptionRepository(_dbHelper);
                
                // Try to insert consumption record with invalid PowerPlantId
                var invalidConsumption = new EnergyConsumption
                {
                    PowerPlantId = -999, // Invalid foreign key
                    RecordDate = DateTime.Now,
                    ConsumptionMWh = 100.0m,
                    PeakDemand = 50.0m,
                    Region = "Test",
                    CostPerMWh = 30.0m,
                    ConsumerType = "Test",
                    CarbonEmissions = 0.0m
                };

                var result = consumptionRepo.InsertConsumptionRecord(invalidConsumption);
                
                // ANTI-PATTERN: Expecting failure but not testing proper error handling
                Assert.IsFalse(result, "Foreign key constraint should prevent invalid insert");
            }
            catch (Exception)
            {
                // ANTI-PATTERN: Catching exceptions as expected behavior
                Assert.IsTrue(true, "Expected foreign key constraint violation");
            }
        }

        [TestMethod]
        public void DatabasePerformance_BasicQuery_ShouldBeReasonable()
        {
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                // ANTI-PATTERN: Performance testing without proper baseline
                var repository = new PowerPlantRepository(_dbHelper);
                var plants = repository.GetAllPowerPlants();
                
                stopwatch.Stop();
                
                // ANTI-PATTERN: Arbitrary performance threshold
                Assert.IsTrue(stopwatch.ElapsedMilliseconds < 5000, 
                    $"Query took too long: {stopwatch.ElapsedMilliseconds}ms");
                
                Assert.IsNotNull(plants);
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"Performance test failed: {ex.Message}");
            }
        }

        [TestMethod]
        public void SqlInjectionVulnerability_ShouldExist()
        {
            try
            {
                // ANTI-PATTERN: Test that demonstrates security vulnerability
                var repository = new PowerPlantRepository(_dbHelper);
                
                // This should be vulnerable to SQL injection
                string maliciousInput = "Solar' OR '1'='1";
                var result = repository.GetPowerPlantsByType(maliciousInput);
                
                // ANTI-PATTERN: Test shows vulnerability exists
                // If SQL injection works, this might return all plants instead of just Solar
                Assert.IsNotNull(result);
                
                // This test demonstrates the security issue for modernization
            }
            catch (Exception ex)
            {
                // ANTI-PATTERN: SQL injection might cause syntax errors
                StringAssert.Contains(ex.Message.ToLower(), "syntax", 
                    "SQL injection attempt caused syntax error");
            }
        }

        [TestMethod]
        public void ConcurrentAccess_ShouldHandleMultipleConnections()
        {
            // ANTI-PATTERN: Inadequate concurrency testing
            var tasks = new List<Task>();
            var exceptions = new List<Exception>();

            for (int i = 0; i < 5; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var repository = new PowerPlantRepository(_dbHelper);
                        var plants = repository.GetAllPowerPlants();
                        Assert.IsNotNull(plants);
                    }
                    catch (Exception ex)
                    {
                        lock (exceptions)
                        {
                            exceptions.Add(ex);
                        }
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());

            if (exceptions.Count > 0)
            {
                Assert.Fail($"Concurrent access failed: {exceptions.First().Message}");
            }
        }

        [TestMethod]
        public void DataConsistency_AfterCRUDOperations_ShouldMaintainIntegrity()
        {
            try
            {
                var repository = new PowerPlantRepository(_dbHelper);
                var originalCount = repository.GetAllPowerPlants().Count;

                // ANTI-PATTERN: Test modifies actual database
                var testPlant = new PowerPlant
                {
                    Name = "Integration Test Plant",
                    Type = "Test",
                    Capacity = 50.0m,
                    Location = "Test Location",
                    CommissionDate = DateTime.Now,
                    Status = "Active",
                    CurrentOutput = 40.0m,
                    EfficiencyRating = 0.8m,
                    OperatorCompany = "Test Company",
                    MaintenanceCost = 25000.0m,
                    LastMaintenanceDate = DateTime.Now
                };

                // Insert
                var insertResult = repository.InsertPowerPlant(testPlant);
                Assert.IsTrue(insertResult);

                var newCount = repository.GetAllPowerPlants().Count;
                Assert.AreEqual(originalCount + 1, newCount);

                // ANTI-PATTERN: No proper cleanup - leaves test data in database
            }
            catch (Exception ex)
            {
                Assert.Fail($"Data consistency test failed: {ex.Message}");
            }
        }

        // ANTI-PATTERN: Missing integration tests for:
        // - Transaction rollback scenarios
        // - Database connection pooling
        // - Deadlock handling
        // - Large dataset performance
        // - Memory usage under load
        // - Connection timeout scenarios
        // - Database failover testing
        // - Backup and restore procedures
        // - Data migration scenarios
        // - Cross-database compatibility

        [TestCleanup]
        public void Cleanup()
        {
            // ANTI-PATTERN: No proper cleanup of test data
            // Integration tests should clean up after themselves
        }
    }
}
