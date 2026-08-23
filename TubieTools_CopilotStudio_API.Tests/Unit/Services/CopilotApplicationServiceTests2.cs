using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TubieTools_CopilotStudio_API.Services;
using TubieTools_CopilotStudio_API.Services.DTOs;
using TubieTools_CopilotStudio_API.Data.Repositories;
using TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

namespace TubieTools_CopilotStudio_API.Tests.Unit.Services
{
    [TestClass]
    [Description("Unit tests for CopilotApplicationService")]
    public class CopilotApplicationServiceTests
    {
        private Mock<ICopilotApplicationRepository> _mockRepository;
        private CopilotApplicationService _service;

        [TestInitialize]
        public void Setup()
        {
            _mockRepository = new Mock<ICopilotApplicationRepository>();
            _service = new CopilotApplicationService(_mockRepository.Object);
        }

        [TestMethod]
        [Description("CreateAsync persists entity and returns DTO")]
        public async Task CreateAsync_WithValidRequest_PersistsAndReturnsDto()
        {
            var request = new CreateCopilotRequest(
                Name: "TestApp",
                Description: null,
                BusinessObjective: null,
                LandingZone: "Zone-A",
                Owner: null,
                ContactEmail: null
            );
            _mockRepository
                .Setup(r => r.AddAsync(It.IsAny<CopilotApplication>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CopilotApplication app) => app);

            var result = await _service.CreateAsync(request, CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.AreEqual("TestApp", result.Name);
            _mockRepository.Verify(
                r => r.AddAsync(It.IsAny<CopilotApplication>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [TestMethod]
        [Description("GetByIdAsync returns DTO when entity found")]
        public async Task GetByIdAsync_WhenFound_ReturnsDto()
        {
            var entity = new CopilotApplication
            {
                CopilotId = "app-1",
                Name = "TestApp",
                CurrentVersion = "1.0.0",
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };
            _mockRepository
                .Setup(r => r.GetByIdAsync("app-1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            var result = await _service.GetByIdAsync("app-1", CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.AreEqual("TestApp", result.Name);
            Assert.AreEqual("app-1", result.CopilotId);
        }

        [TestMethod]
        [Description("GetAllAsync returns list of DTOs")]
        public async Task GetAllAsync_ReturnsAllApplications()
        {
            var entities = new List<CopilotApplication>
            {
                new CopilotApplication 
                { 
                    CopilotId = "1",
                    Name = "App1",
                    CurrentVersion = "1.0",
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                },
                new CopilotApplication 
                { 
                    CopilotId = "2",
                    Name = "App2",
                    CurrentVersion = "1.0",
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                }
            };
            _mockRepository
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(entities);

            var result = await _service.GetAllAsync(CancellationToken.None);

            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        [Description("DeleteAsync removes entity")]
        public async Task DeleteAsync_RemovesEntity()
        {
            string id = "app-1";
            _mockRepository
                .Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await _service.DeleteAsync(id, CancellationToken.None);

            _mockRepository.Verify(
                r => r.DeleteAsync(id, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
