using LuxotticaSorting.ApplicationServices.DivertLanes;
using LuxotticaSorting.ApplicationServices.Shared.DTO.DivertLanes;
using LuxotticaSorting.Controllers.DivertLanes;
using LuxotticaSorting.DataAccess.Repositories.DivertLanes;
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

namespace LuxxoticaSorting.Unitest.DivertLanesTest
{
    [TestFixture]
    public class DivertLanesTest
    {
        private DivertLanesController _controller;
        private Mock<IDivertLanesAppService> _divertLanesAppService;
        private Mock<ILogger<DivertLanesController>> _logger;

        [SetUp]
        public void Setup()
        {
            _divertLanesAppService = new Mock<IDivertLanesAppService>();
            _logger = new Mock<ILogger<DivertLanesController>>();
            _controller = new DivertLanesController(_divertLanesAppService.Object, _logger.Object);
        }

        [Test]
        public void GetAll_ReturnsOkResult()
        {
            // Arrange
            _divertLanesAppService.Setup(s => s.GetDivertLanesAsync()).ReturnsAsync(new List<DivertLanesDTO>());

            // Act
            var result = _controller.GetAll().Result as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public void GetDivertlanesToPrintGaylordAll_ReturnsOkResult()
        {
            // Arrange
            _divertLanesAppService.Setup(s => s.GetDivertLanesToPrintGaylordAsync()).ReturnsAsync(new List<DivertLanesDTO>());

            // Act
            var result = _controller.GetAll().Result as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public void GetDivertlanesToPrintTruckAll_ReturnsOkResult()
        {
            // Arrange
            _divertLanesAppService.Setup(s => s.GetDivertLanesToPrintTruckAsync()).ReturnsAsync(new List<DivertLanesDTO>());

            // Act
            var result = _controller.GetAll().Result as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public void GetById_ExistingId_ReturnsOkResult()
        {
            // Arrange
            var id = 1;
            var divertLaneDto = new DivertLanesDTO { Id = id };
            _divertLanesAppService.Setup(s => s.GetDivertLaneByIdAsync(id)).ReturnsAsync(divertLaneDto);

            // Act
            var result = _controller.GetById(id).Result as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(divertLaneDto, result.Value);
        }



        [Test]
        public void Post_ValidModel_ReturnsOkResult()
        {
            // Arrange
            var entity = new DivertLanesAddDto();
            //_divertLanesAppService.Setup(s => s.AddDivertLaneAsync(entity)).Returns((Task<int>)Task.CompletedTask);
            _divertLanesAppService.Setup(s => s.AddDivertLaneAsync(It.IsAny<DivertLanesAddDto>())).ReturnsAsync(1);


            // Act
            var result = _controller.Post(entity).Result as OkObjectResult;
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);


            var responseMessage = okResult.Value;
            Assert.NotNull(responseMessage);
        }

        [Test]
        public void Post_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            DivertLanesAddDto entity = null; // Invalid model

            // Act
            var result = _controller.Post(entity).Result as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.AreEqual("Invalid JSON Model!.", result.Value);
        }

        [Test]
        public void Put_ValidModel_ReturnsOkResult()
        {
            // Arrange
            var id = 1;
            var entity = new DivertLanesAddDto();
            _divertLanesAppService.Setup(s => s.EditDivertLaneAsync(id, entity)).Returns(Task.CompletedTask);

            // Act
            var result = _controller.Put(id, entity).Result as OkObjectResult;

            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);


            var responseMessage = okResult.Value;
            Assert.NotNull(responseMessage);
        }

        [Test]
        public void Put_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var id = 1;
            DivertLanesAddDto entity = null; // Invalid model

            // Act
            var result = _controller.Put(id, entity).Result as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.AreEqual("Invalid JSON model!.", result.Value);
        }

        [Test]
        public void Delete_ExistingId_ReturnsOkResult()
        {
            // Arrange
            var id = 1;
            _divertLanesAppService.Setup(s => s.DeleteDivertLaneAsync(id)).Returns(Task.CompletedTask);

            // Act
            var result = _controller.Delete(id).Result as OkObjectResult;

            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);


            var responseMessage = okResult.Value;
            Assert.NotNull(responseMessage);
        }

        [Test]
        public void Delete_NonExistingId_ReturnsBadRequest()
        {
            // Arrange
            var id = 0;

            // Act
            var result = _controller.Delete(id).Result as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.AreEqual("Id is null!.", result.Value);
        }
    }
}
