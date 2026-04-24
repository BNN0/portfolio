using LuxotticaSorting.ApplicationServices.Mapping.CarrierCodeDivertLaneMapping;
using LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodeDivertLaneMapping;
using LuxotticaSorting.Controllers.Mapping.CarrierCodeDivertLaneMapping;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LuxxoticaSorting.Unitest.CarrierCodeDivertLaneMappings
{
    [TestFixture]
    public class CarrierCodeDivertLaneMappingTest
    {
        private CarrierCodeDivertLaneMappingController _controller;
        private Mock<ICarrierCodeDivertLaneMappingAppService> _service;
        private Mock<ILogger<CarrierCodeDivertLaneMappingController>> _logger;

        [SetUp]
        public void Setup()
        {
            _service = new Mock<ICarrierCodeDivertLaneMappingAppService>();
            _logger = new Mock<ILogger<CarrierCodeDivertLaneMappingController>>();
            _controller = new CarrierCodeDivertLaneMappingController(_service.Object, _logger.Object);
        }

        [Test]
        public async Task GetAll()
        {
            // Arrange
            _service.Setup(s => s.GetCarrierCodeDivertLaneMappingAsync()).ReturnsAsync(new List<CarrierCodeDivertLaneMappingDto>());

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
            _service.Setup(s => s.GetCarrierCodeDivertLaneMappingByIdAsync(id)).ReturnsAsync(new CarrierCodeDivertLaneMappingDto());

            // Act
            var result = await _controller.GetById(id);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task GetAllCombinedData()
        {
            // Arrange
            _service.Setup(s => s.GetCombinedDataAsync()).ReturnsAsync(new List<CarrierCodeDivertLaneMappingGetAllDto>());

            // Act
            var result = await _controller.GetAllCombinedData();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task Post()
        {
            // Arrange
            var entity = new CarrierCodeDivertLaneMappingAddDto
            {
                CarrierCodeId = 1,
                DivertLaneId = 2,
                Status = false
            };

            _service.Setup(service => service.AddCarrierCodeDivertLaneMappingAsync(It.IsAny<CarrierCodeDivertLaneMappingAddDto>())).Returns(Task.CompletedTask);

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
            var entity = new CarrierCodeDivertLaneMappingEditDto
            {
                Status = false
            };

            _service.Setup(s => s.EditCarrierCodeDivertLaneMappingAsync(id, entity)).Returns(Task.CompletedTask);

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
            _service.Setup(s => s.DeleteCarrierCodeDivertLaneMappingAsync(id)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(id) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }
    }
}
