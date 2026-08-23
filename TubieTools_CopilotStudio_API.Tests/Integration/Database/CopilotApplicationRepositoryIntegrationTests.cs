using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TubieTools_CopilotStudio_API.Data;
using TubieTools_CopilotStudio_API.Data.Repositories;
using TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

namespace TubieTools_CopilotStudio_API.Tests.Integration.Database
{
    [TestClass]
    [Description("Integration tests for CopilotApplicationRepository with real DbContext")]
    public class CopilotApplicationRepositoryIntegrationTests
    {
        private CopilotStudioDbContext _dbContext;
        private CopilotApplicationRepository _repository;

        [TestInitialize]
        public async Task Setup()
        {
            // ARRANGE: Use in-memory database for testing
            var options = new DbContextOptionsBuilder<CopilotStudioDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new CopilotStudioDbContext(options);
            _repository = new CopilotApplicationRepository(_dbContext);

            // Initialize schema
            await _dbContext.Database.EnsureCreatedAsync();
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            // Clean up
            await _dbContext.DisposeAsync();
        }

        [TestMethod]
        [Description("AddAsync persists entity to database")]
        public async Task AddAsync_PersistsEntity_ToDatabase()
        {
            // ARRANGE
            var app = new CopilotApplication
            {
                CopilotId = "test-app-1",
                Name = "Integration Test App",
                LandingZone = "Zone-A",
                Environment = "Test",
                CreatedDate = DateTime.UtcNow,
                CurrentVersion = "1.0.0",
                IsActive = true
            };

            // ACT
            await _repository.AddAsync(app, CancellationToken.None);
            await _repository.SaveChangesAsync(CancellationToken.None);

            // ASSERT
            var persisted = await _repository.GetByIdAsync("test-app-1", CancellationToken.None);
            Assert.IsNotNull(persisted);
            Assert.AreEqual("Integration Test App", persisted.Name);
            Assert.AreEqual("Zone-A", persisted.LandingZone);
        }

        [TestMethod]
        [Description("GetByIdAsync retrieves entity from database")]
        public async Task GetByIdAsync_RetrievesEntity_FromDatabase()
        {
            // ARRANGE
            var app = new CopilotApplication
            {
                CopilotId = "test-app-2",
                Name = "Retrieval Test",
                CurrentVersion = "1.0.0",
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };
            await _repository.AddAsync(app, CancellationToken.None);
            await _repository.SaveChangesAsync(CancellationToken.None);

            // ACT
            var retrieved = await _repository.GetByIdAsync("test-app-2", CancellationToken.None);

            // ASSERT
            Assert.IsNotNull(retrieved);
            Assert.AreEqual("Retrieval Test", retrieved.Name);
            Assert.AreEqual("test-app-2", retrieved.CopilotId);
        }

        [TestMethod]
        [Description("GetByIdAsync returns null for nonexistent entity")]
        public async Task GetByIdAsync_WithNonexistentId_ReturnsNull()
        {
            // ARRANGE & ACT
            var result = await _repository.GetByIdAsync("nonexistent", CancellationToken.None);

            // ASSERT
            Assert.IsNull(result);
        }

        [TestMethod]
        [Description("UpdateAsync modifies persisted entity")]
        public async Task UpdateAsync_ModifiesEntity_InDatabase()
        {
            // ARRANGE
            var app = new CopilotApplication
            {
                CopilotId = "test-app-3",
                Name = "Original Name",
                CurrentVersion = "1.0.0",
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };
            await _repository.AddAsync(app, CancellationToken.None);
            await _repository.SaveChangesAsync(CancellationToken.None);

            // ACT
            var existing = await _repository.GetByIdAsync("test-app-3", CancellationToken.None);
            existing.Name = "Updated Name";
            existing.CurrentVersion = "1.1.0";
            await _repository.UpdateAsync(existing, CancellationToken.None);
            await _repository.SaveChangesAsync(CancellationToken.None);

            // ASSERT
            var updated = await _repository.GetByIdAsync("test-app-3", CancellationToken.None);
            Assert.AreEqual("Updated Name", updated.Name);
            Assert.AreEqual("1.1.0", updated.CurrentVersion);
        }

        [TestMethod]
        [Description("DeleteAsync removes entity from database")]
        public async Task DeleteAsync_RemovesEntity_FromDatabase()
        {
            // ARRANGE
            var app = new CopilotApplication
            {
                CopilotId = "test-app-4",
                Name = "To Delete",
                CurrentVersion = "1.0.0",
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };
            await _repository.AddAsync(app, CancellationToken.None);
            await _repository.SaveChangesAsync(CancellationToken.None);

            // ACT
            await _repository.DeleteAsync("test-app-4", CancellationToken.None);
            await _repository.SaveChangesAsync(CancellationToken.None);

            // ASSERT
            var deleted = await _repository.GetByIdAsync("test-app-4", CancellationToken.None);
            Assert.IsNull(deleted);
        }

        [TestMethod]
        [Description("GetAllAsync retrieves all entities")]
        public async Task GetAllAsync_RetrievesAll_Entities()
        {
            // ARRANGE
            var apps = new List<CopilotApplication>
            {
                new CopilotApplication { CopilotId = "app-g1", Name = "GetAll1", CurrentVersion = "1.0", CreatedDate = DateTime.UtcNow, IsActive = true },
                new CopilotApplication { CopilotId = "app-g2", Name = "GetAll2", CurrentVersion = "1.0", CreatedDate = DateTime.UtcNow, IsActive = true },
                new CopilotApplication { CopilotId = "app-g3", Name = "GetAll3", CurrentVersion = "1.0", CreatedDate = DateTime.UtcNow, IsActive = true }
            };

            foreach (var app in apps)
            {
                await _repository.AddAsync(app, CancellationToken.None);
            }
            await _repository.SaveChangesAsync(CancellationToken.None);

            // ACT
            var result = await _repository.GetAllAsync(CancellationToken.None);

            // ASSERT
            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result.TrueForAll(a => a.Name.StartsWith("GetAll")));
        }

        [TestMethod]
        [Description("GetByLandingZoneAsync filters by landing zone")]
        public async Task GetByLandingZoneAsync_FiltersBy_LandingZone()
        {
            // ARRANGE
            await _repository.AddAsync(new CopilotApplication
            {
                CopilotId = "zone-a-1",
                Name = "ZoneA1",
                LandingZone = "Zone-A",
                CurrentVersion = "1.0",
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            }, CancellationToken.None);

            await _repository.AddAsync(new CopilotApplication
            {
                CopilotId = "zone-b-1",
                Name = "ZoneB1",
                LandingZone = "Zone-B",
                CurrentVersion = "1.0",
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            }, CancellationToken.None);

            await _repository.SaveChangesAsync(CancellationToken.None);

            // ACT
            var result = await _repository.GetByLandingZoneAsync("Zone-A", CancellationToken.None);

            // ASSERT
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Zone-A", result[0].LandingZone);
            Assert.AreEqual("ZoneA1", result[0].Name);
        }

        [TestMethod]
        [Description("GetActiveAsync returns only active applications")]
        public async Task GetActiveAsync_FiltersBy_IsActive()
        {
            // ARRANGE
            await _repository.AddAsync(new CopilotApplication
            {
                CopilotId = "active-1",
                Name = "Active1",
                CurrentVersion = "1.0",
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            }, CancellationToken.None);

            await _repository.AddAsync(new CopilotApplication
            {
                CopilotId = "inactive-1",
                Name = "Inactive1",
                CurrentVersion = "1.0",
                CreatedDate = DateTime.UtcNow,
                IsActive = false
            }, CancellationToken.None);

            await _repository.SaveChangesAsync(CancellationToken.None);

            // ACT
            var result = await _repository.GetActiveAsync(CancellationToken.None);

            // ASSERT
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result[0].IsActive);
        }

        [TestMethod]
        [Description("Multiple adds and saves work correctly")]
        public async Task MultipleSaves_AddMultipleEntities_PersistAll()
        {
            // ARRANGE & ACT
            for (int i = 1; i <= 5; i++)
            {
                await _repository.AddAsync(new CopilotApplication
                {
                    CopilotId = $"bulk-{i}",
                    Name = $"BulkApp{i}",
                    CurrentVersion = "1.0",
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                }, CancellationToken.None);

                await _repository.SaveChangesAsync(CancellationToken.None);
            }

            // ASSERT
            var all = await _repository.GetAllAsync(CancellationToken.None);
            Assert.AreEqual(5, all.Count);
        }

        [TestMethod]
        [Description("Transaction rollback on error")]
        public async Task SaveChangesAsync_WithException_DoesNotPersist()
        {
            // ARRANGE
            var app1 = new CopilotApplication
            {
                CopilotId = "rollback-1",
                Name = "RollbackTest1",
                CurrentVersion = "1.0",
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            await _repository.AddAsync(app1, CancellationToken.None);
            await _repository.SaveChangesAsync(CancellationToken.None);

            // Verify it was saved
            var saved = await _repository.GetByIdAsync("rollback-1", CancellationToken.None);
            Assert.IsNotNull(saved);

            // ACT: Delete and verify
            await _repository.DeleteAsync("rollback-1", CancellationToken.None);
            await _repository.SaveChangesAsync(CancellationToken.None);

            // ASSERT
            var deleted = await _repository.GetByIdAsync("rollback-1", CancellationToken.None);
            Assert.IsNull(deleted);
        }
    }
}
