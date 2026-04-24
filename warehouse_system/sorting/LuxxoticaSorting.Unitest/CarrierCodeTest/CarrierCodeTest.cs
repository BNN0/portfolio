using LuxotticaSorting.ApplicationServices.CarriersCodes;
using LuxotticaSorting.ApplicationServices.Shared.DTO.BoxTypes;
using LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodes;
using LuxotticaSorting.Controllers.CarrierCodes;
using LuxotticaSorting.Core.CarrierCodes;
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

namespace LuxxoticaSorting.Unitest.CarrierCodeTest
{
    public class CarrierCodeTest
    {
        private CarrierCodeController _controller;
        private Mock<ICarrierCodeAppService> _mockAppService;
        private Mock<ILogger<CarrierCodeController>> _mockLogger;

        [SetUp]
        public void Setup()
        {
            _mockAppService = new Mock<ICarrierCodeAppService>();
            _mockLogger = new Mock<ILogger<CarrierCodeController>>();
            _controller = new CarrierCodeController(_mockAppService.Object, _mockLogger.Object);
        }

        [Test]
        public async Task GetAll_ReturnsOk()
        {
            // Arrange
            _mockAppService.Setup(service => service.GetCarrierCodesAsync()).ReturnsAsync(new List<CarrierCode>());

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
        }

        [Test]
        public async Task GetById_ReturnsOk()
        {
            // Arrange
            int id = 1;
            var carrierCode = new CarrierCode { /* Initialize with necessary properties */ };
            _mockAppService.Setup(service => service.GetCarrierCodeByIdAsync(id)).ReturnsAsync(carrierCode);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
        }

        [Test]
        public async Task GetById_ReturnsNotFound()
        {
            // Arrange
            int id = 1;
            _mockAppService.Setup(service => service.GetCarrierCodeByIdAsync(id)).ReturnsAsync((CarrierCode)null);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        [Test]
        public async Task Post_ValidModel_ShouldReturnOkResult()
        {
            // Arrange
            var entity = new CarrierCodeAddDto
            {
                CarrierCodes = "CarrierCode1"
            };


            int simulatedEntityId = 1;
            _mockAppService
                .Setup(service => service.AddCarrierCodeAsync(It.IsAny<CarrierCodeAddDto>()))
                .ReturnsAsync(simulatedEntityId);

            // Act
            var result = await _controller.Post(entity);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);


            var responseMessage = okResult.Value;
            Assert.NotNull(responseMessage);

        }

        [Test]
        public async Task Put_ReturnsOk()
        {
            // Arrange
            int id = 1;
            var entity = new CarrierCodeAddDto { /* Initialize with necessary properties */ };
            _mockAppService.Setup(service => service.EditCarrierCodeAsync(id, entity)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Put(id, entity);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
        }

        [Test]
        public async Task Put_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            int id = 1;
            CarrierCodeAddDto entity = null;

            // Act
            var result = await _controller.Put(id, entity);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task Delete_ReturnsOk()
        {
            // Arrange
            int id = 1;
            _mockAppService.Setup(service => service.DeleteCarrierCodeAsync(id)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

    }
}
