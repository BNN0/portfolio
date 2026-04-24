using LuxotticaSorting.ApplicationServices.BoxTypes;
using LuxotticaSorting.ApplicationServices.Shared.DTO.BoxTypes;
using LuxotticaSorting.Controllers.BoxTypes;
using LuxotticaSorting.Core.BoxTypes;
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

namespace LuxxoticaSorting.Unitest.BoxTypesTest
{
    [TestFixture]
    public class BoxTypeTest
    {
        private BoxTypeController _controller;
        private Mock<IBoxTypeAppService> _boxTypeAppService;
        private Mock<ILogger<BoxTypeController>> _logger;

        [SetUp]
        public void Setup()
        {
            _boxTypeAppService = new Mock<IBoxTypeAppService>();
            _logger = new Mock<ILogger<BoxTypeController>>();
            _controller = new BoxTypeController(_boxTypeAppService.Object, _logger.Object);
        }

        [Test]
        public async Task GetAll_ShouldReturnOkResult()
        {
            // Arrange
            _boxTypeAppService.Setup(s => s.GetBoxTypesAsync()).ReturnsAsync(new List<BoxType>());

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task GetById_ExistingId_ShouldReturnOkResult()
        {
            // Arrange
            int id = 1;
            _boxTypeAppService.Setup(s => s.GetBoxTypeByIdAsync(id)).ReturnsAsync(new BoxType());

            // Act
            var result = await _controller.GetById(id);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task Post_ValidModel_ShouldReturnOkResult()
        {
            // Arrange
            var entity = new BoxTypesAddDTO
            {
                BoxTypes = "XDDDD"
            };


            int simulatedEntityId = 1; 
            _boxTypeAppService
                .Setup(service => service.AddBoxTypeAsync(It.IsAny<BoxTypesAddDTO>()))
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
        public async Task Put_ValidIdAndModel_ShouldReturnOkResult()
        {
            // Arrange
            int id = 1;
            var entity = new BoxTypesAddDTO
            {
                BoxTypes="XD"
            };
            _boxTypeAppService.Setup(s => s.EditBoxTypeAsync(id, entity)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Put(id, entity);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task Delete_ValidId_ShouldReturnOkResult()
        {
            // Arrange
            int id = 1;
            _boxTypeAppService.Setup(s => s.DeleteBoxTypeAsync(id)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }
    }
}
