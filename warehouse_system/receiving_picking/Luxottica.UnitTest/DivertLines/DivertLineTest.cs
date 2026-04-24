using Luxottica.ApplicationServices.CameraAssignments;
using Luxottica.ApplicationServices.DivertLines;
using Luxottica.ApplicationServices.Shared.Dto.CameraAssignments;
using Luxottica.ApplicationServices.Shared.Dto.DivertLines;
using Luxottica.Controllers.CameraAssignments;
using Luxottica.Controllers.DivertLines;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.UnitTest.DivertLines
{
    public class DivertLineTest
    {
        private Mock<IDivertLineService> _divertLineServiceMock;
        private DivertLineController _controller;
        private Mock<ILogger<DivertLineController>> _loggerMock;

        [SetUp]
        public void Setup()
        {
            _divertLineServiceMock = new Mock<IDivertLineService>();
            _loggerMock = new Mock<ILogger<DivertLineController>>();
            _controller = new DivertLineController(_divertLineServiceMock.Object, _loggerMock.Object);
        }
        [Test]
        public async Task TestAdd()
        {
            // Arrange
            int divertLineValue = 1;
            bool statusLine = true;
            var newDivertLine = new DivertLineAddDto { DivertLineValue = divertLineValue, StatusLine = statusLine };

            _divertLineServiceMock.Setup(service => service.AddDivertLineAsync(newDivertLine));

            // Act
            var result = await _controller.Post(newDivertLine);

            // Assert
            var okResult = result as OkResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Test]
        public async Task GetAll_()
        {
            // Arrange
            var divertLineDtos = new List<DivertLineDto>
    {
        new DivertLineDto { Id = 1, DivertLineValue = 1, StatusLine = true },
        new DivertLineDto { Id = 2, DivertLineValue = 2, StatusLine = true },
    };

            _divertLineServiceMock.Setup(service => service.GetDivertLineAsync())
                .ReturnsAsync(divertLineDtos);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

            var divertLines = okResult.Value as List<DivertLineDto>;
            Assert.NotNull(divertLines);
            Assert.AreEqual(2, divertLines.Count);
        }

        [Test]
        public async Task GetById()
        {
            // Arrange
            int testId = 1;
            int divertLineValue = 1;
            bool statusLine = true;
            _divertLineServiceMock.Setup(service => service.GetDivertLineByIdAsync(testId))
                .ReturnsAsync(new DivertLineDto { Id = testId, DivertLineValue = divertLineValue, StatusLine = statusLine });

            // Act
            var result = await _controller.GetById(testId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

            var divertLin = okResult.Value as DivertLineDto;
            Assert.NotNull(divertLin);
            Assert.AreEqual(testId, divertLin.Id);
        }
        [Test]
        public async Task TestEdit()
        {
            // Arrange
            int testId = 1;
            var editDivertLine = new DivertLineAddDto { DivertLineValue = 1, StatusLine = false };
            _divertLineServiceMock.Setup(service => service.EditDivertLineAsync(testId, editDivertLine));

            // Act
            var result = await _controller.Put(testId, editDivertLine);

            // Assert
            var okResult = result as OkResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Test]
        public async Task TestDelete()
        {
            // Arrange
            int testId = 1;

            _divertLineServiceMock.Setup(service => service.DeleteDivertLineAsync(testId));

            // Act
            var result = await _controller.Delete(testId);

            // Assert
            var okResult = result as OkResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        }
    }
}
