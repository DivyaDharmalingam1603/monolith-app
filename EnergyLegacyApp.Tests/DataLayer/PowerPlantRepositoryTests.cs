using Microsoft.VisualStudio.TestTools.UnitTesting;
using EnergyLegacyApp.Data;
using EnergyLegacyApp.Data.Models;
using Microsoft.Extensions.Configuration;

namespace EnergyLegacyApp.Tests.DataLayer
{
    [TestClass]
    public class PowerPlantRepositoryTests
    {
        private PowerPlantRepository _repository;

        [TestInitialize]
        public void Setup()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("TestSettings.json")
                .Build();
            var dbHelper = new DatabaseHelper(config);
            _repository = new PowerPlantRepository(dbHelper);
        }

        [TestMethod]
        public void GetAllPowerPlants_ShouldReturnList()
        {
            // ANTI-PATTERN: Test depends on actual database connection
            // This will fail if database is not available
            try
            {
                var result = _repository.GetAllPowerPlants();
                
                // ANTI-PATTERN: Weak assertions
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Count >= 0);
            }
            catch (Exception ex)
            {
                // ANTI-PATTERN: Catching and ignoring database connection errors
                Assert.Inconclusive($"Database connection failed: {ex.Message}");
            }
        }

        [TestMethod]
        public void GetPowerPlantById_WithValidId_ShouldReturnPlant()
        {
            // ANTI-PATTERN: Magic numbers and hardcoded test data
            int testId = 1;
            
            try
            {
                var result = _repository.GetPowerPlantById(testId);
                
                // ANTI-PATTERN: Test assumes specific data exists
                if (result != null)
                {
                    Assert.AreEqual(testId, result.Id);
                    Assert.IsNotNull(result.Name);
                }
                else
                {
                    Assert.Inconclusive("Test data not found in database");
                }
            }
            catch (Exception)
            {
                Assert.Inconclusive("Database connection failed");
            }
        }

        [TestMethod]
        public void GetPowerPlantById_WithInvalidId_ShouldReturnNull()
        {
            // ANTI-PATTERN: Testing with unrealistic data
            int invalidId = -999;
            
            try
            {
                var result = _repository.GetPowerPlantById(invalidId);
                Assert.IsNull(result);
            }
            catch (Exception)
            {
                Assert.Inconclusive("Database connection failed");
            }
        }

        [TestMethod]
        public void InsertPowerPlant_WithValidData_ShouldReturnTrue()
        {
            // ANTI-PATTERN: Test modifies actual database
            var plant = new PowerPlant
            {
                Name = "Test Plant",
                Type = "Solar",
                Capacity = 100.0m,
                Location = "Test Location",
                CommissionDate = DateTime.Now,
                Status = "Active",
                CurrentOutput = 80.0m,
                EfficiencyRating = 0.8m,
                OperatorCompany = "Test Company",
                MaintenanceCost = 50000.0m,
                LastMaintenanceDate = DateTime.Now
            };

            try
            {
                var result = _repository.InsertPowerPlant(plant);
                
                // ANTI-PATTERN: No cleanup after test
                Assert.IsTrue(result);
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"Insert failed: {ex.Message}");
            }
        }

        [TestMethod]
        public void InsertPowerPlant_WithNullName_ShouldHandleGracefully()
        {
            // ANTI-PATTERN: Testing SQL injection vulnerability
            var plant = new PowerPlant
            {
                Name = null, // This will cause issues due to SQL injection vulnerability
                Type = "Solar",
                Capacity = 100.0m,
                Location = "Test Location",
                CommissionDate = DateTime.Now,
                Status = "Active"
            };

            try
            {
                var result = _repository.InsertPowerPlant(plant);
                // ANTI-PATTERN: Test doesn't verify proper error handling
                Assert.IsFalse(result);
            }
            catch (Exception)
            {
                // ANTI-PATTERN: Expecting exceptions instead of proper validation
                Assert.IsTrue(true, "Expected exception due to null name");
            }
        }

        [TestMethod]
        public void GetPowerPlantsByType_WithValidType_ShouldReturnFilteredList()
        {
            string testType = "Solar";
            
            try
            {
                var result = _repository.GetPowerPlantsByType(testType);
                
                Assert.IsNotNull(result);
                
                // ANTI-PATTERN: Weak validation of filtering logic
                foreach (var plant in result)
                {
                    Assert.AreEqual(testType, plant.Type);
                }
            }
            catch (Exception)
            {
                Assert.Inconclusive("Database connection failed");
            }
        }

        [TestMethod]
        public void GetPowerPlantsByType_WithSqlInjection_ShouldBeVulnerable()
        {
            // ANTI-PATTERN: Test demonstrates SQL injection vulnerability
            string maliciousInput = "Solar'; DROP TABLE PowerPlants; --";
            
            try
            {
                // This test shows the vulnerability exists
                var result = _repository.GetPowerPlantsByType(maliciousInput);
                
                // ANTI-PATTERN: Test doesn't properly validate security
                Assert.IsNotNull(result);
            }
            catch (Exception ex)
            {
                // ANTI-PATTERN: Catching SQL errors without proper handling
                StringAssert.Contains(ex.Message.ToLower(), "syntax");
            }
        }

        [TestMethod]
        public void UpdatePowerPlant_WithValidData_ShouldReturnTrue()
        {
            // ANTI-PATTERN: Test depends on existing data
            try
            {
                var existingPlant = _repository.GetPowerPlantById(1);
                if (existingPlant != null)
                {
                    existingPlant.CurrentOutput = 95.0m;
                    var result = _repository.UpdatePowerPlant(existingPlant);
                    Assert.IsTrue(result);
                }
                else
                {
                    Assert.Inconclusive("No test data available");
                }
            }
            catch (Exception)
            {
                Assert.Inconclusive("Database operation failed");
            }
        }

        [TestMethod]
        public void DeletePowerPlant_WithValidId_ShouldReturnTrue()
        {
            // ANTI-PATTERN: Destructive test without proper setup/teardown
            // This test should not run in production environment
            
            // First insert a test record
            var testPlant = new PowerPlant
            {
                Name = "Test Plant for Deletion",
                Type = "Wind",
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

            try
            {
                var insertResult = _repository.InsertPowerPlant(testPlant);
                if (insertResult)
                {
                    // ANTI-PATTERN: Assuming the last inserted record has a specific ID
                    // This is unreliable in concurrent environments
                    var plants = _repository.GetAllPowerPlants();
                    var lastPlant = plants.LastOrDefault(p => p.Name == "Test Plant for Deletion");
                    
                    if (lastPlant != null)
                    {
                        var deleteResult = _repository.DeletePowerPlant(lastPlant.Id);
                        Assert.IsTrue(deleteResult);
                    }
                }
            }
            catch (Exception)
            {
                Assert.Inconclusive("Delete test failed due to database issues");
            }
        }

        // ANTI-PATTERN: No cleanup method
        // [TestCleanup] method is missing
    }
}
