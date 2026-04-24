using LuxotticaSorting.ApplicationServices.Mapping.DivertLaneZebraConfigurations;
using LuxotticaSorting.ApplicationServices.Shared.DTO.DivertLaneZebraConfigurationMapping;
using LuxotticaSorting.Controllers.DivertLaneZebraConfigurationMappings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxxoticaSorting.Unitest.DivertLaneZebraConfigurationMappingTest
{
    [TestFixture]
    public class DivertLaneZebraConfigurationMappingsControllerTests
    {
        private DivertLaneZebraConfigurationMappingsController _controller;
        private Mock<IDivertLaneZebraConfigurationAppService> _appService;
        private Mock<ILogger<DivertLaneZebraConfigurationMappingsController>> _logger;

        [SetUp]
        public void Setup()
        {
            _appService = new Mock<IDivertLaneZebraConfigurationAppService>();
            _logger = new Mock<ILogger<DivertLaneZebraConfigurationMappingsController>>();
            _controller = new DivertLaneZebraConfigurationMappingsController(_appService.Object, _logger.Object);
        }

        [Test]
        public async Task GetAll_ReturnsOk()
        {
            // Arrange
            var expectedMappings = new List<DivertLaneZebraConfigurationMappingDTO> { new DivertLaneZebraConfigurationMappingDTO() };
            _appService.Setup(service => service.GetDivertLaneZebraConfigurationMappingAsync()).ReturnsAsync(expectedMappings);

            // Act
            var result = await _controller.GetAll() as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
            Assert.AreEqual(expectedMappings, result.Value);
        }


        [Test]
        public async Task GetCombinatedDataAll_ReturnsOk()
        {
            // Arrange
            var expectedMappings = new List<DivertLanesZebraConfigurationCombinated> { new DivertLanesZebraConfigurationCombinated() };
            _appService.Setup(service => service.GetDivertLaneZebraConfigurationMappingCombinatedDataAsync()).ReturnsAsync(expectedMappings);

            // Act
            var result = await _controller.GetCombinatedData() as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
            Assert.AreEqual(expectedMappings, result.Value);
        }

        [Test]
        public async Task GetById_WhenMappingExists_ReturnsOkWithMapping()
        {
            // Arrange
            int mappingId = 1;
            var expectedMapping = new DivertLaneZebraConfigurationMappingDTO();
            _appService.Setup(service => service.GetDivertLaneZebraConfigurationMappingByIdAsync(mappingId)).ReturnsAsync(expectedMapping);

            // Act
            var result = await _controller.GetById(mappingId) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
            Assert.AreEqual(expectedMapping, result.Value);
        }

        [Test]
        public async Task Post_WhenModelIsValid_ReturnsOkWithMessage()
        {
            // Arrange
            var mappingToAdd = new DivertLaneZebraConfigurationMappingAddDTO();
            _appService
                .Setup(service => service.AddDivertLaneZebraConfigurationMappingAsync(It.IsAny<DivertLaneZebraConfigurationMappingAddDTO>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Post(mappingToAdd) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
            Assert.NotNull(result.Value);
        }

        [Test]
        public async Task Put_WhenModelIsValid_ReturnsOkWithMessage()
        {
            // Arrange
            int mappingId = 1;
            var mappingToEdit = new DivertLaneZebraConfigurationMappingAddDTO();
            _appService.Setup(service => service.EditDivertLaneZebraConfigurationMappingAsync(mappingId, mappingToEdit)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Put(mappingId, mappingToEdit) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
            Assert.NotNull(result.Value);
        }

        [Test]
        public async Task Put_WhenIdOrModelIsNull_ReturnsBadRequest()
        {
            // Arrange
            int mappingId = 1;
            _appService.Setup(service => service.EditDivertLaneZebraConfigurationMappingAsync(mappingId, null)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Put(mappingId, null) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid JSON model!.", result.Value);
        }

        [Test]
        public async Task Delete_WhenIdIsValid_ReturnsOkWithMessage()
        {
            // Arrange
            int mappingId = 1;
            _appService.Setup(service => service.DeleteDivertLaneZebraConfigurationMappingAsync(mappingId)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(mappingId) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
            Assert.NotNull(result.Value);
        }

        [Test]
        public async Task Delete_WhenIdIsZero_ReturnsBadRequest()
        {
            // Arrange
            int mappingId = 0;

            // Act
            var result = await _controller.Delete(mappingId) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Id is null!.", result.Value);
        }
    }
}
