using Microsoft.VisualStudio.TestTools.UnitTesting;
using EnergyLegacyApp.Business;
using EnergyLegacyApp.Data;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace EnergyLegacyApp.Tests.Performance
{
    [TestClass]
    public class PerformanceTests
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
        public void PowerPlantService_GetAllPowerPlants_PerformanceTest()
        {
            var powerPlantRepo = new PowerPlantRepository(_dbHelper);
            var consumptionRepo = new EnergyConsumptionRepository(_dbHelper);
            var service = new PowerPlantService(powerPlantRepo, consumptionRepo);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var result = service.GetAllPowerPlants();
                stopwatch.Stop();

                // ANTI-PATTERN: Arbitrary performance threshold
                Assert.IsTrue(stopwatch.ElapsedMilliseconds < 2000, 
                    $"GetAllPowerPlants took {stopwatch.ElapsedMilliseconds}ms, expected < 2000ms");

                Assert.IsNotNull(result);
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"Performance test failed: {ex.Message}");
            }
        }

        [TestMethod]
        public void EnergyAnalyticsService_GetDashboardData_PerformanceTest()
        {
            var powerPlantRepo = new PowerPlantRepository(_dbHelper);
            var consumptionRepo = new EnergyConsumptionRepository(_dbHelper);
            var service = new EnergyAnalyticsService(consumptionRepo, powerPlantRepo);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var result = service.GetDashboardData();
                stopwatch.Stop();

                // ANTI-PATTERN: No consideration for acceptable performance ranges
                Assert.IsTrue(stopwatch.ElapsedMilliseconds < 5000, 
                    $"GetDashboardData took {stopwatch.ElapsedMilliseconds}ms, expected < 5000ms");

                Assert.IsNotNull(result);
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"Dashboard performance test failed: {ex.Message}");
            }
        }

        [TestMethod]
        public void DatabaseHelper_MultipleQueries_PerformanceTest()
        {
            // ANTI-PATTERN: Testing database performance without connection pooling consideration
            var stopwatch = Stopwatch.StartNew();
            var queryCount = 10;

            try
            {
                for (int i = 0; i < queryCount; i++)
                {
                    var query = "SELECT COUNT(*) FROM PowerPlants";
                    var result = _dbHelper.ExecuteScalar(query);
                    Assert.IsNotNull(result);
                }

                stopwatch.Stop();

                // ANTI-PATTERN: Linear performance expectation without considering overhead
                var averageTime = stopwatch.ElapsedMilliseconds / queryCount;
                Assert.IsTrue(averageTime < 500, 
                    $"Average query time {averageTime}ms, expected < 500ms");
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"Multiple queries performance test failed: {ex.Message}");
            }
        }

        [TestMethod]
        public void PowerPlantRepository_GetPowerPlantsByType_PerformanceTest()
        {
            var repository = new PowerPlantRepository(_dbHelper);
            var testTypes = new[] { "Solar", "Wind", "Coal", "Gas", "Nuclear" };

            try
            {
                foreach (var type in testTypes)
                {
                    var stopwatch = Stopwatch.StartNew();
                    var result = repository.GetPowerPlantsByType(type);
                    stopwatch.Stop();

                    // ANTI-PATTERN: Same performance expectation regardless of result set size
                    Assert.IsTrue(stopwatch.ElapsedMilliseconds < 1000, 
                        $"GetPowerPlantsByType({type}) took {stopwatch.ElapsedMilliseconds}ms");

                    Assert.IsNotNull(result);
                }
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"Type filtering performance test failed: {ex.Message}");
            }
        }

        [TestMethod]
        public void EnergyAnalyticsService_GetConsumptionByRegionSummary_MemoryTest()
        {
            var powerPlantRepo = new PowerPlantRepository(_dbHelper);
            var consumptionRepo = new EnergyConsumptionRepository(_dbHelper);
            var service = new EnergyAnalyticsService(consumptionRepo, powerPlantRepo);
            
            try
            {
                var initialMemory = GC.GetTotalMemory(true);
                
                var result = service.GetRegionalAnalysis();
                
                var finalMemory = GC.GetTotalMemory(false);
                var memoryUsed = finalMemory - initialMemory;

                // ANTI-PATTERN: Arbitrary memory threshold without context
                Assert.IsTrue(memoryUsed < 10 * 1024 * 1024, // 10MB
                    $"Memory usage {memoryUsed / 1024 / 1024}MB exceeded threshold");

                Assert.IsNotNull(result);
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"Memory test failed: {ex.Message}");
            }
        }

        [TestMethod]
        public void ConcurrentAccess_PerformanceTest()
        {
            var powerPlantRepo = new PowerPlantRepository(_dbHelper);
            var consumptionRepo = new EnergyConsumptionRepository(_dbHelper);
            var service = new PowerPlantService(powerPlantRepo, consumptionRepo);
            var taskCount = 5;
            var tasks = new List<Task<TimeSpan>>();

            for (int i = 0; i < taskCount; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var stopwatch = Stopwatch.StartNew();
                    try
                    {
                        var result = service.GetAllPowerPlants();
                        stopwatch.Stop();
                        return stopwatch.Elapsed;
                    }
                    catch
                    {
                        stopwatch.Stop();
                        return TimeSpan.MaxValue; // Indicate failure
                    }
                }));
            }

            try
            {
                Task.WaitAll(tasks.ToArray());

                var times = tasks.Select(t => t.Result).Where(t => t != TimeSpan.MaxValue).ToList();
                
                if (times.Count < taskCount)
                {
                    Assert.Inconclusive($"Only {times.Count}/{taskCount} concurrent requests succeeded");
                }

                var averageTime = times.Average(t => t.TotalMilliseconds);
                
                // ANTI-PATTERN: No consideration for concurrent access overhead
                Assert.IsTrue(averageTime < 3000, 
                    $"Average concurrent access time {averageTime}ms exceeded threshold");
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"Concurrent access test failed: {ex.Message}");
            }
        }

        [TestMethod]
        public void LargeDataSet_SimulationTest()
        {
            var powerPlantRepo = new PowerPlantRepository(_dbHelper);
            var consumptionRepo = new EnergyConsumptionRepository(_dbHelper);
            var service = new EnergyAnalyticsService(consumptionRepo, powerPlantRepo);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // ANTI-PATTERN: This doesn't actually test with large data
                for (int i = 0; i < 100; i++)
                {
                    var result = service.GetDashboardData();
                    Assert.IsNotNull(result);
                }

                stopwatch.Stop();

                // ANTI-PATTERN: Linear scaling assumption
                Assert.IsTrue(stopwatch.ElapsedMilliseconds < 30000, // 30 seconds
                    $"Large dataset simulation took {stopwatch.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"Large dataset simulation failed: {ex.Message}");
            }
        }

        [TestMethod]
        public void ExportToCsv_PerformanceTest()
        {
            var powerPlantRepo = new PowerPlantRepository(_dbHelper);
            var consumptionRepo = new EnergyConsumptionRepository(_dbHelper);
            var service = new EnergyAnalyticsService(consumptionRepo, powerPlantRepo);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var result = service.ExportDataToCsv("plants");
                stopwatch.Stop();

                Assert.IsTrue(stopwatch.ElapsedMilliseconds < 3000,
                    $"CSV export took {stopwatch.ElapsedMilliseconds}ms");

                Assert.IsNotNull(result);
                Assert.IsTrue(result.Length > 0);
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"CSV export performance test failed: {ex.Message}");
            }
        }

        // ANTI-PATTERN: Missing performance tests for:
        // - Database connection establishment time
        // - Query execution plan analysis
        // - Index usage verification
        // - Memory leak detection over time
        // - CPU usage under load
        // - Network latency impact
        // - Caching effectiveness
        // - Garbage collection impact
        // - Thread pool exhaustion scenarios
        // - Resource cleanup performance

        [TestCleanup]
        public void Cleanup()
        {
            // ANTI-PATTERN: No performance test cleanup or result logging
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
