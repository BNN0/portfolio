using LuxotticaSorting.ApplicationServices.NewBoxs;
using LuxotticaSorting.ApplicationServices.Shared.DTO.NewBoxs;
using LuxotticaSorting.Controllers.NewBoxs;
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
using Xunit;

namespace LuxxoticaSorting.Unitest.NewBoxs
{
    [TestFixture]
    public class NewBoxTest
    {
        private NewBoxController _controller;
        private Mock<INewBoxAppService> _newBoxAppService;
        private Mock<ILogger<NewBoxController>> _logger;

        [SetUp]
        public void Setup()
        {
            _newBoxAppService = new Mock<INewBoxAppService>();
            _logger = new Mock<ILogger<NewBoxController>>();
            _controller = new NewBoxController(_newBoxAppService.Object, _logger.Object);
        }

        [Test]
        public void Post_ReturnsOkResult()
        {
            // Arrange
            var entity = new NewBoxAddDto();
            _newBoxAppService.Setup(s => s.AddNewBoxAsync(entity)).Returns(Task.CompletedTask);

            // Act
            var result = _controller.Post(entity).Result as OkObjectResult;

            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

            var responseMessage = okResult.Value as dynamic;
            Assert.NotNull(responseMessage);
        }


        [Test]
        public void Post_ReturnsBadRequest()
        {
            // Arrange
            NewBoxAddDto entity = null;

            // Act
            var result = _controller.Post(entity).Result as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.AreEqual("Invalid JSON Model!.", result.Value);
        }

        [Test]
        public void Post_ReturnsInternalServerError()
        {
            // Arrange
            var entity = new NewBoxAddDto();
            _newBoxAppService.Setup(s => s.AddNewBoxAsync(entity)).Throws(new Exception("Test Exception"));

            // Act
            var result = _controller.Post(entity).Result as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.AreEqual("Test Exception", result.Value);
        }
    }
}
