using Luxottica.ApplicationServices.CameraAssignments;
using Luxottica.ApplicationServices.PhysicalMaps;
using Luxottica.ApplicationServices.Shared.Dto;
using Luxottica.ApplicationServices.Shared.Dto.CameraAssignments;
using Luxottica.Controllers.CameraAssignments;
using Luxottica.Controllers.PhysicalMaps;
using Luxottica.Core.Entities.CameraAssignments;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.UnitTest.CameraAssignments
{
    public class CameraAssignmentTest
    {
        private Mock<ICameraAssignmentService> _cameraAssignmentServiceMock;
        private CameraAssignmentController _controller;
        private Mock<ILogger<CameraAssignmentController>> _loggerMock;

        [SetUp]
        public void Setup()
        {
            _cameraAssignmentServiceMock = new Mock<ICameraAssignmentService>();
            _loggerMock = new Mock<ILogger<CameraAssignmentController>>();
            _controller = new CameraAssignmentController(_cameraAssignmentServiceMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task TestAdd()
        {
            // Arrange
            int divertLineId = 1;
            int cameraId = 1;
            var newCameraAssignment = new CameraAssignmentAddDto { DivertLineId = divertLineId, CameraId = cameraId };

            _cameraAssignmentServiceMock.Setup(service => service.AddCameraAssignmentAsync(newCameraAssignment));

            // Act
            await _controller.Post(newCameraAssignment);

            // Assert
            _cameraAssignmentServiceMock.Verify(service => service.AddCameraAssignmentAsync(newCameraAssignment), Times.Once);
        }

        [Test]
        public async Task TestGetAll()
        {
            // Arrange
            var expectedCameraAssignments = new List<CameraAssignmentDto>
    {
        new CameraAssignmentDto { Id = 1, DivertLineId = 3, CameraId = 3 },
        new CameraAssignmentDto { Id = 2, DivertLineId = 4, CameraId = 4 },
    };

            _cameraAssignmentServiceMock.Setup(service => service.GetCameraAssignmentAsync())
                .ReturnsAsync(expectedCameraAssignments);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

            var cameraAssignment = okResult.Value as List<CameraAssignmentDto>;
            Assert.NotNull(cameraAssignment);
            Assert.AreEqual(expectedCameraAssignments.Count, cameraAssignment.Count);
        }

        [Test]
        public async Task TestGetById()
        {
            // Arrange
            int testId = 1;
            int divertLineId = 3;
            int cameraId = 3;
            _cameraAssignmentServiceMock.Setup(service => service.GetCameraAssignmentByIdAsync(testId))
                .ReturnsAsync(new CameraAssignmentDto { Id = testId, DivertLineId = divertLineId, CameraId = cameraId });

            // Act
            var result = await _controller.GetById(testId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

            var cameraAssignment = okResult.Value as CameraAssignmentDto;
            Assert.NotNull(cameraAssignment);
            Assert.AreEqual(testId, cameraAssignment.Id);
        }


        [Test]
        public async Task TestEdit()
        {
            // Arrange
            int testId = 1;
            var editCameraAssignment = new CameraAssignmentAddDto { DivertLineId = 1, CameraId = 10 };
            _cameraAssignmentServiceMock.Setup(service => service.EditCameraAssignmentAsync(testId, editCameraAssignment));

            // Act
            await _controller.Put(testId, editCameraAssignment);

            // Assert
            _cameraAssignmentServiceMock.Verify(service => service.EditCameraAssignmentAsync(testId, editCameraAssignment), Times.Once);
        }

        [Test]
        public async Task TestDelete()
        {
            // Arrange
            int testId = 1;
            _cameraAssignmentServiceMock.Setup(service => service.DeleteCameraAssignmentAsync(testId));

            // Act
            await _controller.Delete(testId);

            // Assert
            _cameraAssignmentServiceMock.Verify(service => service.DeleteCameraAssignmentAsync(testId), Times.Once);
        }
    }
}
