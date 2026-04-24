using AutoMapper;
using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.Mapping.CarrierCodeLogisticAgent;
using LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodeDivertLaneMapping;
using LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodeLogisticAgent;
using LuxotticaSorting.Controllers.Mapping.CarrierCodeLogisticAgent;
using LuxotticaSorting.Core.Mapping.CarrierCodeLogisticAgent;
using LuxotticaSorting.DataAccess.Repositories.Mapping.CarrierCodeLogisticAgent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LuxxoticaSorting.Unitest.CarrierCodeLogisticAgentMapping
{
    [TestFixture]
    public class CarrierCodeLogisticAgentMappingTest
    {
        private CarrierCodeLogisticAgentController _controller;
        private Mock<ICarrierCodeLogisticAgentAppService> _service;
        private Mock<ILogger<CarrierCodeLogisticAgentController>> _logger;

        [SetUp]
        public void Setup()
        {
            _service = new Mock<ICarrierCodeLogisticAgentAppService>();
            _logger = new Mock<ILogger<CarrierCodeLogisticAgentController>>();
            _controller = new CarrierCodeLogisticAgentController(_service.Object, _logger.Object);
        }

        [Test]
        public async Task GetAll()
        {
            // Arrange
            _service.Setup(s => s.GetCarrierCodeLogisticAgentAsync()).ReturnsAsync(new List<CarrierCodeLogisticAgentDto>());

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task GetById()
        {
            // Arrange
            int id = 1;
            _service.Setup(s => s.GetCarrierCodeLogisticAgentByIdAsync(id)).ReturnsAsync(new CarrierCodeLogisticAgentDto());

            // Act
            var result = await _controller.GetById(id);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }
        [Test]
        public async Task Post()
        {
            // Arrange
            var entity = new CarrierCodeLogisticAgentAddDto
            {
                CarrierCodeId = 1,
                LogisticAgentId = 2,
            };

            _service.Setup(service => service.AddCarrierCodeLogisticAgentAsync(It.IsAny<CarrierCodeLogisticAgentAddDto>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Post(entity) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task Put()
        {
            // Arrange
            int id = 1;
            var entity = new CarrierCodeLogisticAgentAddDto
            {
                CarrierCodeId = 1,
                LogisticAgentId = 2,
            };

            _service.Setup(s => s.EditCarrierCodeLogisticAgentAsync(id, entity)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Put(id, entity) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task Delete()
        {
            // Arrange
            int id = 1;
            _service.Setup(s => s.DeleteCarrierCodeLogisticAgentAsync(id)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(id) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task GetCombinedData()
        {
            // Arrange
            _service.Setup(s => s.GetCombinedDataAsync()).ReturnsAsync(new List<CarrierCodeLogisticAgentGetAllDto>());

            // Act
            var result = await _controller.GetCombinedData();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }
    }
}
