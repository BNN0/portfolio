using LuxotticaSorting.ApplicationServices.Containers;
using LuxotticaSorting.ApplicationServices.Shared.DTO.Containers;
using LuxotticaSorting.ApplicationServices.Shared.DTO.ZebraHistorial;
using LuxotticaSorting.ApplicationServices.ZebraHistorial;
using LuxotticaSorting.Controllers.Containers;
using LuxotticaSorting.Controllers.ZebraHistorial;
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

namespace LuxxoticaSorting.Unitest.ZebraHistorial
{
    [TestFixture]
    public class ZebraHistorialControllerTests
    {
        private ZebraHistorialController _zebraHistorialController;
        private Mock<IZebraHistorialAppService> _zebraHistorialAppService;
        private Mock<ILogger<ZebraHistorialController>> _logger;
        [SetUp]
        public void Setup()
        {
            _zebraHistorialAppService = new Mock<IZebraHistorialAppService>();
            _logger = new Mock<ILogger<ZebraHistorialController>>();
            _zebraHistorialController = new ZebraHistorialController(_zebraHistorialAppService.Object, _logger.Object);
        }

        [Test]
        public async Task GetAll_WhenServiceSucceeds_ReturnsOkWithZebra()
        {
            // Arrange
            var expected = new List<ZebraHistorialDTO> { new ZebraHistorialDTO() };
            _zebraHistorialAppService.Setup(service => service.GetZebraHistorialsAsync()).ReturnsAsync(expected);

            // Act
            var result = await _zebraHistorialController.GetAll() as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(expected, result.Value);
        }

        [Test]
        public async Task GetAll_WhenServiceSucceeds_ReturnsOkGaylordRePrint()
        {
            // Arrange
            var expected = new List<ZebraHistorialData> { new ZebraHistorialData() };
            _zebraHistorialAppService.Setup(service => service.GetZebraHistorialsToRePrintAsync()).ReturnsAsync(expected);

            // Act
            var result = await _zebraHistorialController.GetToRePrintAll() as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(expected, result.Value);
        }

        [Test]
        public async Task GetAll_WhenServiceSucceeds_ReturnsOkTruckRePrint()
        {
            // Arrange
            var expected = new List<ZebraHistorialData> { new ZebraHistorialData() };
            _zebraHistorialAppService.Setup(service => service.GetZebraHistorialTruckAsync()).ReturnsAsync(expected);

            // Act
            var result = await _zebraHistorialController.GetTruckAll() as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(expected, result.Value);
        }

        [Test]
        public async Task GetAll_WhenServiceSucceeds_ReturnsOkGaylordData()
        {
            // Arrange
            var expected = new List<ZebraHistorialData> { new ZebraHistorialData() };
            _zebraHistorialAppService.Setup(service => service.GetZebraHistorialGaylordAsync()).ReturnsAsync(expected);

            // Act
            var result = await _zebraHistorialController.GetGaylordAll() as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(expected, result.Value);
        }

        [Test]
        public async Task GetAll_WhenServiceSucceeds_ReturnsOkCombinatedData()
        {
            // Arrange
            var expected = new List<ZebraHistorialData> { new ZebraHistorialData() };
            _zebraHistorialAppService.Setup(service => service.GetZebraHistorialDataCombinatedAsync()).ReturnsAsync(expected);

            // Act
            var result = await _zebraHistorialController.GetCombinatedDataAll() as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(expected, result.Value);
        }


        [Test]
        public async Task GetById_WhenContainerExists_ReturnsOkWithContainer()
        {
            // Arrange
            int ZebraId = 1;
            var Zebra = new ZebraHistorialDTO();
            _zebraHistorialAppService.Setup(service => service.GetZebraHistorialByIdAsync(ZebraId)).ReturnsAsync(Zebra);

            // Act
            var result = await _zebraHistorialController.GetById(ZebraId) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(Zebra, result.Value);
        }


        [Test]
        public async Task Delete_WhenIdIsValid_ReturnsOkWithMessage()
        {
            // Arrange
            _zebraHistorialAppService.Setup(service => service.DeleteZebraHistorialsAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _zebraHistorialController.Delete() as ObjectResult;

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }
    }
}
