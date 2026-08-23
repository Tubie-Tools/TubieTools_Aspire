using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TubieTools_CopilotStudio_API.Controllers;
using TubieTools_CopilotStudio_API.Services;
using TubieTools_CopilotStudio_API.Services.DTOs;

namespace TubieTools_CopilotStudio_API.Tests.Unit.Controllers
{
    [TestClass]
    [Description("Unit tests for CopilotApplicationsController")]
    public class CopilotApplicationsControllerTests
    {
        private Mock<ICopilotApplicationService> _mockService;
        private CopilotApplicationsController _controller;

        [TestInitialize]
        public void Setup()
        {
            // ARRANGE: Initialize mocks and controller
            _mockService = new Mock<ICopilotApplicationService>();
            _controller = new CopilotApplicationsController(_mockService.Object);
        }

        [TestMethod]
        [Description("GetAll returns list when applications exist")]
        public async Task GetAll_WhenApplicationsExist_ReturnsList()
        {
            // ARRANGE
            var applications = new List<CopilotApplicationDto>
            {
                new(CopilotId: "app-1", Name: "TestApp1", CurrentVersion: "1.0.0"),
                new(CopilotId: "app-2", Name: "TestApp2", CurrentVersion: "1.0.1")
            };
            _mockService
                .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(applications);

            // ACT
            var result = await _controller.GetAll();

            // ASSERT
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("TestApp1", result[0].Name);
            Assert.AreEqual("TestApp2", result[1].Name);
            _mockService.Verify(
                s => s.GetAllAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [TestMethod]
        [Description("GetAll returns empty list when no applications exist")]
        public async Task GetAll_WhenNoApplicationsExist_ReturnsEmptyList()
        {
            // ARRANGE
            var emptyList = new List<CopilotApplicationDto>();
            _mockService
                .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyList);

            // ACT
            var result = await _controller.GetAll();

            // ASSERT
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [Description("GetById returns application when found")]
        public async Task GetById_WhenApplicationFound_ReturnsApplication()
        {
            // ARRANGE
            string copilotId = "app-1";
            var app = new CopilotApplicationDto(
                CopilotId: copilotId,
                Name: "TestApp",
                CurrentVersion: "1.0.0"
            );
            _mockService
                .Setup(s => s.GetByIdAsync(copilotId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(app);

            // ACT
            var result = await _controller.GetById(copilotId);

            // ASSERT
            Assert.IsNotNull(result);
            Assert.AreEqual("TestApp", result.Name);
            Assert.AreEqual("app-1", result.CopilotId);
        }

        [TestMethod]
        [Description("GetById returns null when application not found")]
        public async Task GetById_WhenApplicationNotFound_ReturnsNull()
        {
            // ARRANGE
            string copilotId = "nonexistent";
            _mockService
                .Setup(s => s.GetByIdAsync(copilotId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((CopilotApplicationDto)null);

            // ACT
            var result = await _controller.GetById(copilotId);

            // ASSERT
            Assert.IsNull(result);
            _mockService.Verify(
                s => s.GetByIdAsync(copilotId, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [TestMethod]
        [Description("GetByLandingZone returns filtered applications")]
        public async Task GetByLandingZone_FiltersApplications_ByZone()
        {
            // ARRANGE
            string zone = "Zone-A";
            var apps = new List<CopilotApplicationDto>
            {
                new(CopilotId: "app-za-1", Name: "ZoneAApp1", CurrentVersion: "1.0.0"),
                new(CopilotId: "app-za-2", Name: "ZoneAApp2", CurrentVersion: "1.0.0")
            };
            _mockService
                .Setup(s => s.GetByLandingZoneAsync(zone, It.IsAny<CancellationToken>()))
                .ReturnsAsync(apps);

            // ACT
            var result = await _controller.GetByLandingZone(zone);

            // ASSERT
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.TrueForAll(a => a.Name.Contains("ZoneA")));
        }

        [TestMethod]
        [Description("Create returns created application with DTO")]
        public async Task Create_WithValidRequest_ReturnsCreatedApplication()
        {
            // ARRANGE
            var request = new CreateCopilotRequest(
                Name: "NewApp",
                Description: null,
                BusinessObjective: null,
                LandingZone: "Zone-A",
                Owner: null,
                ContactEmail: null
            );
            var createdApp = new CopilotApplicationDto(
                CopilotId: "app-new",
                Name: "NewApp",
                Description: null,
                BusinessObjective: null,
                LandingZone: "Zone-A",
                Owner: null,
                ContactEmail: null,
                CurrentVersion: "0.0.1",
                IsActive: true,
                CreatedDate: DateTime.UtcNow,
                LastModifiedDate: DateTime.UtcNow
            );
            _mockService
                .Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdApp);

            // ACT
            var result = await _controller.Create(request);

            // ASSERT
            Assert.IsNotNull(result);
            Assert.AreEqual("NewApp", result.Name);
            Assert.AreEqual("0.0.1", result.CurrentVersion);
            _mockService.Verify(
                s => s.CreateAsync(request, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [TestMethod]
        [Description("Update modifies application")]
        public async Task Update_WithValidRequest_ReturnsUpdatedApplication()
        {
            // ARRANGE
            string copilotId = "app-1";
            var request = new UpdateCopilotApplicationRequest(Name: "UpdatedName");
            var updatedApp = new CopilotApplicationDto(
                CopilotId: copilotId,
                Name: "UpdatedName",
                CurrentVersion: "1.0.1"
            );
            _mockService
                .Setup(s => s.UpdateAsync(copilotId, request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedApp);

            // ACT
            var result = await _controller.Update(copilotId, request);

            // ASSERT
            Assert.IsNotNull(result);
            Assert.AreEqual("UpdatedName", result.Name);
            _mockService.Verify(
                s => s.UpdateAsync(copilotId, request, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [TestMethod]
        [Description("Delete removes application")]
        public async Task Delete_WithValidId_CallsServiceDelete()
        {
            // ARRANGE
            string copilotId = "app-1";
            _mockService
                .Setup(s => s.DeleteAsync(copilotId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // ACT
            await _controller.Delete(copilotId);

            // ASSERT
            _mockService.Verify(
                s => s.DeleteAsync(copilotId, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [TestMethod]
        [Description("Create with null request handles gracefully")]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Create_WithNullRequest_ThrowsArgumentNullException()
        {
            // ARRANGE & ACT & ASSERT
            await _controller.Create(null);
        }

        [TestMethod]
        [Description("GetById with empty ID handles gracefully")]
        public async Task GetById_WithEmptyId_CallsServiceWithEmptyId()
        {
            // ARRANGE
            _mockService
                .Setup(s => s.GetByIdAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync((CopilotApplicationDto)null);

            // ACT
            var result = await _controller.GetById(string.Empty);

            // ASSERT
            Assert.IsNull(result);
        }
    }
}
