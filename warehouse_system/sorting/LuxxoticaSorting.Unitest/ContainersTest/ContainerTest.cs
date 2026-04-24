using LuxotticaSorting.ApplicationServices.Containers;
using LuxotticaSorting.ApplicationServices.Shared.DTO.Containers;
using LuxotticaSorting.Controllers.Containers;
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

namespace LuxxoticaSorting.Unitest.ContainersTest
{
    [TestFixture]
    public class ContainerTest
    {
        private ContainerController _containerController;
        private Mock<IContainerAppService> _containerAppService;
        private Mock<ILogger<ContainerController>> _logger;
        [SetUp]
        public void Setup()
        {
            _containerAppService = new Mock<IContainerAppService>();
            _logger = new Mock<ILogger<ContainerController>>();
            _containerController = new ContainerController(_containerAppService.Object, _logger.Object);
        }

        [Test]
        public async Task GetAll_WhenServiceSucceeds_ReturnsOkWithContainers()
        {
            // Arrange
            var expectedContainers = new List<ContainerDTO> { new ContainerDTO() };
            _containerAppService.Setup(service => service.GetContainersAsync()).ReturnsAsync(expectedContainers);

            // Act
            var result = await _containerController.GetAll() as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(expectedContainers, result.Value);
        }


        [Test]
        public async Task GetById_WhenContainerExists_ReturnsOkWithContainer()
        {
            // Arrange
            int containerId = 1;
            var expectedContainer = new ContainerDTO();
            _containerAppService.Setup(service => service.GetContainerByIdAsync(containerId)).ReturnsAsync(expectedContainer);

            // Act
            var result = await _containerController.GetById(containerId) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(expectedContainer, result.Value);
        }


        [Test]
        public async Task GetTruckAll_WhenServiceSucceeds_ReturnsOkWithContainers()
        {
            // Arrange
            var expectedContainers = new List<ContainerToShow> { new ContainerToShow() };
            _containerAppService.Setup(service => service.GetContainersTruckAsync()).ReturnsAsync(expectedContainers);

            // Act
            var result = await _containerController.GetTruckAll() as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(expectedContainers, result.Value);
        }

        [Test]
        public async Task GetGaylordAll_WhenServiceSucceeds_ReturnsOkWithContainers()
        {
            // Arrange
            var expectedContainers = new List<ContainerToShow> { new ContainerToShow() };
            _containerAppService.Setup(service => service.GetContainersGaylordAsync()).ReturnsAsync(expectedContainers);

            // Act
            var result = await _containerController.GetGaylordAll() as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(expectedContainers, result.Value);
        }

        [Test]
        public async Task Post_WhenModelIsValid_ReturnsOkWithMessage()
        {
            // Arrange
            var containerToAdd = new ContainerAddDTO
            {
                ContainerId= "Container01",
                ContainerTypeId= 1
            };
            int simulatedEntityId = 1;
            _containerAppService
                .Setup(service => service.AddContainerAsync(It.IsAny<ContainerAddDTO>()))
                .ReturnsAsync(simulatedEntityId);

            // Act
            var result = await _containerController.Post(containerToAdd);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);


            var responseMessage = okResult.Value;
            Assert.NotNull(responseMessage);
        }

        [Test]
        public async Task PostContainerToPrint_WhenModelIsValid_ReturnsOkWithMessage()
        {
            // Arrange
            var containerToAdd = new ContainerToPrint
            {
                ContainerId = "1000000001",
                DivertLanesId = 1
            };

            int simulatedEntityId = 1;
            _containerAppService
                .Setup(service => service.AddContainerToPrintAsync(It.IsAny<ContainerToPrint>()))
                .ReturnsAsync(true);

            // Act
            var result = await _containerController.PostToPrint(containerToAdd);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);


            var responseMessage = okResult.Value;
            Assert.NotNull(responseMessage);
        }

        [Test]
        public async Task Put_WhenModelIsValid_ReturnsOkWithMessage()
        {
            // Arrange
            int containerId = 1;
            var containerToEdit = new ContainerAddDTO();
            _containerAppService.Setup(service => service.EditContainerAsync(containerId, containerToEdit)).Returns(Task.CompletedTask);

            // Act
            var result = await _containerController.Put(containerId, containerToEdit) as ObjectResult;

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task Put_WhenIdOrModelIsNull_ReturnsBadRequest()
        {
            // Arrange
            int containerId = 1;
            _containerAppService.Setup(service => service.EditContainerAsync(containerId, null)).Returns(Task.CompletedTask);

            // Act
            var result = await _containerController.Put(containerId, null) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }



        [Test]
        public async Task Delete_WhenIdIsValid_ReturnsOkWithMessage()
        {
            // Arrange
            int containerId = 1;
            _containerAppService.Setup(service => service.DeleteContainerAsync(containerId)).Returns(Task.CompletedTask);

            // Act
            var result = await _containerController.Delete(containerId) as ObjectResult;

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task Delete_WhenIdIsZero_ReturnsBadRequest()
        {
            // Arrange
            int containerId = 0;

            // Act
            var result = await _containerController.Delete(containerId) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        [Test]
        public async Task PostGayLord_WhenModelIsValid_ReturnsOkWithMessage()
        {
            // Arrange
            var containerToAdd = new ContainerAddOneStepDTO
            {
                ContainerTypeId = 1
            };
            int simulatedEntityId = 1;
            _containerAppService
                .Setup(service => service.AddContainerGayLordAsync(It.IsAny<ContainerAddOneStepDTO>()))
                .ReturnsAsync(simulatedEntityId);

            // Act
            var result = await _containerController.PostGayLord(containerToAdd);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);


            var responseMessage = okResult.Value;
            Assert.NotNull(responseMessage);
        }
        [Test]
        public async Task PostTruck_WhenModelIsValid_ReturnsOkWithMessage()
        {
            // Arrange
            var containerToAdd = new ContainerAddOneStepDTO
            {
                ContainerTypeId = 1
            };
            int simulatedEntityId = 1;
            _containerAppService
                .Setup(service => service.AddContainerTruckAsync(It.IsAny<ContainerAddOneStepDTO>()))
                .ReturnsAsync(simulatedEntityId);

            // Act
            var result = await _containerController.PostTruck(containerToAdd);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);


            var responseMessage = okResult.Value;
            Assert.NotNull(responseMessage);
        }

    }
}
