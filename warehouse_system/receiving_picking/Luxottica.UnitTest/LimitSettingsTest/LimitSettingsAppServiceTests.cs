using AutoMapper;
using Luxottica.ApplicationServices.LimitSettings;
using Luxottica.ApplicationServices.Shared.Dto.LimitSettings;
using Luxottica.Controllers.LimitSettings;
using Luxottica.Controllers.SeconLevelCameraAssignment;
using Luxottica.Core.Entities.LimitsSettings;
using Luxottica.DataAccess.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.UnitTest.LimitSettingsTest
{
    [TestFixture]
    public class LimitSettingsControllerTests
    {
        private LimitSettingsController _controller;
        private Mock<ILimitSettingsAppService> _limitSettingsServiceMock;

        [SetUp]
        public void Setup()
        {
            // Configuración de objetos simulados (Mock) necesarios para las pruebas
            _limitSettingsServiceMock = new Mock<ILimitSettingsAppService>();
            var loggerMock = new Mock<ILogger<LimitSettingsController>>();
            _controller = new LimitSettingsController(
                _limitSettingsServiceMock.Object, loggerMock.Object
            );
        }

        [Test]
        public async Task GetAll_Should_Return_All_LimitSettings()
        {
            // Arrange
            var limitSettings = new List<LimitSettingDTO>
            {
                new LimitSettingDTO { Id = 1 },
                new LimitSettingDTO { Id = 2 },
                new LimitSettingDTO { Id = 3 }
            };

            _limitSettingsServiceMock.Setup(service => service.GetLimitSettingsAsync())
                .ReturnsAsync(limitSettings);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsNotNull(result); 
            Assert.IsInstanceOf<IEnumerable<LimitSettingDTO>>(result); 
            Assert.AreEqual(limitSettings.Count, result.Count()); 
        }

        [Test]
        public async Task GetById_Should_Return_LimitSetting_By_Id()
        {
            // Arrange
            var limitSettingId = 1;
            var limitSetting = new LimitSettingDTO
            {
                Id = limitSettingId,
                CameraId = 101,
                MaximumCapacity = 20,
                CounterTote = 12
            };

            _limitSettingsServiceMock.Setup(service => service.GetLimitSettingByIdAsync(limitSettingId))
                .ReturnsAsync(limitSetting);

            // Act
            var result = await _controller.GetById(limitSettingId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(limitSettingId, result.Id);
        }
        [Test]
        public async Task Delete_Should_Return_InternalServerError_When_Exception()
        {
            // Arrange
            var limitSettingId = 1;

            _limitSettingsServiceMock.Setup(service => service.DeleteLimitSettingAsync(limitSettingId))
                .ThrowsAsync(new Exception("Simulated exception"));

            // Act
            var result = await _controller.Delete(limitSettingId);

            // Assert
            Assert.IsInstanceOf<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(500, objectResult.StatusCode);
        }
        [Test]
        public async Task Post_Should_Return_OkResult_When_AddedSuccessfully()
        {
            // Arrange
            var newLimitSetting = new LimitSettingDTO
            {
                CameraId = 101,
                MaximumCapacity = 20,
                CounterTote = 12
            };

            var addedLimitSettingId = 1;

            _limitSettingsServiceMock.Setup(service => service.AddLimitSettingsAsync(newLimitSetting))
                .ReturnsAsync(addedLimitSettingId);

            // Act
            var result = await _controller.Post(newLimitSetting);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(addedLimitSettingId, okResult.Value);
        }
        [Test]
        public async Task Put_Should_Return_InternalServerError_When_Exception()
        {
            // Arrange
            var limitSettingId = 1;
            var updatedLimitSetting = new LimitSettingDTO
            {
                Id = limitSettingId,
                CameraId = 101,
                MaximumCapacity = 20,
                CounterTote = 12
            };

            _limitSettingsServiceMock.Setup(service => service.EditLimitSettingAsync(updatedLimitSetting))
                .ThrowsAsync(new Exception("Simulated exception"));

            // Act & Assert
            Assert.ThrowsAsync<Exception>(() => _controller.Put(limitSettingId, updatedLimitSetting));
        }


    }

}

