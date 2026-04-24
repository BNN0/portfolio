using LuxotticaSorting.ApplicationServices.ContainerTypes;
using LuxotticaSorting.ApplicationServices.Shared.Dto.ContainerTypes;
using LuxotticaSorting.Controllers.ContainerTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LuxxoticaSorting.Unitest.ContainerTypesTest
{
    public class ContainerTypeTest
    {
        private Mock<ILogger<ContainerTypeController>> _loggerContainerTypeMock;
        private Mock<IContainerTypeAppService> _containerTypeServiceMock;
        private ContainerTypeController _containerTypecontroller;

        [SetUp]
        public void Setup()
        {
            _loggerContainerTypeMock = new Mock<ILogger<ContainerTypeController>>();
            _containerTypeServiceMock = new Mock<IContainerTypeAppService>();
            _containerTypecontroller = new ContainerTypeController(_containerTypeServiceMock.Object, _loggerContainerTypeMock.Object);
        }

        [Test]
        public async Task TestAddContainerType()
        {
            // Arrange
            var newcontainerType = new ContainerTypeAddDto { ContainerTypes = "CONT0001" };

            _containerTypeServiceMock.Setup(service => service.AddContainerTypeAsync(newcontainerType));


            // Act
            var result = await _containerTypecontroller.Post(newcontainerType);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Test]
        public async Task GetAllContainerType()
        {
            // Arrange
            var containerTypeDto = new List<ContainerTypeDto>
                {
                    new ContainerTypeDto { Id = 1, ContainerTypes = "CONT0001" },
                    new ContainerTypeDto { Id = 2, ContainerTypes = "CONT0002"  },
                };

            _containerTypeServiceMock.Setup(service => service.GetContainerTypesAsync())
                .ReturnsAsync(containerTypeDto);

            // Act
            var result = await _containerTypecontroller.GetAll();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

            var divertLines = okResult.Value as List<ContainerTypeDto>;
            Assert.NotNull(divertLines);
            Assert.AreEqual(2, divertLines.Count);
        }

        [Test]
        public async Task GetByIdContainerType()
        {
            // Arrange
            int testId = 1;
            _containerTypeServiceMock.Setup(service => service.GetContainerTypeByIdAsync(testId))
                .ReturnsAsync(new ContainerTypeDto { Id = testId, ContainerTypes = "CONT0001" });

            // Act
            var result = await _containerTypecontroller.GetById(testId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

            var containerType = okResult.Value as ContainerTypeDto;
            Assert.NotNull(containerType);
            Assert.AreEqual(testId, containerType.Id);
        }
        [Test]
        public async Task TestEditContainerType()
        {
            // Arrange
            int testId = 1;
            var editContainerType = new ContainerTypeAddDto { ContainerTypes = "CONT00000N1" };
            _containerTypeServiceMock.Setup(service => service.EditContainerTypeAsync(testId, editContainerType));

            // Act
            var result = await _containerTypecontroller.Put(testId, editContainerType);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Test]
        public async Task TestDeleteContainerType()
        {
            // Arrange
            int testId = 1;

            _containerTypeServiceMock.Setup(service => service.DeleteContainerTypeAsync(testId));

            // Act
            var result = await _containerTypecontroller.Delete(testId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        }
    }
}
