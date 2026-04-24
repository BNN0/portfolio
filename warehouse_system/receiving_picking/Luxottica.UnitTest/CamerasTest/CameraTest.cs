using Luxottica.ApplicationServices.Cameras;
using Luxottica.Controllers.Cameras;
using Luxottica.Core.Entities.Cameras;
using Luxottica.DataAccess.Repositories;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.UnitTest.CamerasTest
{
    using NUnit.Framework;
    using Moq;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Luxottica.ApplicationServices.Shared.Dto.Camera;
    using Microsoft.Extensions.Logging;

    public class CameraTests
    {
        private Mock<ICameraAppService> _cameraServiceMock;
        private CameraController _controller;
        private Mock<ILogger<CameraController>> _loggerMock;
        [SetUp]
        public void Setup()
        {
            _cameraServiceMock = new Mock<ICameraAppService>();
            _loggerMock = new Mock<ILogger<CameraController>>();
            _controller = new CameraController(_cameraServiceMock.Object,_loggerMock.Object);
        }

        [Test]
        public async Task TestGetAll()
        {
            // Arrange
            _cameraServiceMock.Setup(service => service.GetCamerasAsync())
                .ReturnsAsync(new List<CameraDTO>());

            // Act
            var result = await _controller.GetAll();

            // Assert
            var cameras = result as List<CameraDTO>;
            Assert.NotNull(cameras);
            Assert.IsEmpty(cameras);
        }

        [Test]
        public async Task TestGetById()
        {
            // Arrange
            int testId = 1;
            _cameraServiceMock.Setup(service => service.GetCameraByIdAsync(testId))
                .ReturnsAsync(new CameraDTO { Id = testId, CamId = "CAM01" });

            // Act
            var result = await _controller.GetById(testId);

            // Assert
            var camera = result as CameraDTO;
            Assert.NotNull(camera);
            Assert.AreEqual(testId, camera.Id);
        }
        [Test]
        public async Task TestEdit()
        {
            // Arrange
            int testId = 1;
            var editedCamera = new CameraDTO { Id = testId, CamId = "CAM011" };
            _cameraServiceMock.Setup(service => service.EditCameraAsync(editedCamera));

            // Act
            await _controller.Put(testId, editedCamera);

            // Assert
            _cameraServiceMock.Verify(service => service.EditCameraAsync(editedCamera), Times.Once);
        }

        [Test]
        public async Task TestDelete()
        {
            // Arrange
            int testId = 1;
            _cameraServiceMock.Setup(service => service.DeleteCameraAsync(testId));

            // Act
            await _controller.Delete(testId);

            // Assert
            _cameraServiceMock.Verify(service => service.DeleteCameraAsync(testId), Times.Once);
        }

        [Test]
        public async Task TestAdd()
        {
            // Arrange
            var newCamera = new CameraDTO { CamId = "CAM002" };
            _cameraServiceMock.Setup(service => service.AddCameraAsync(newCamera));

            // Act
            var result = await _controller.Post(newCamera);

            // Assert
            Assert.NotNull(result);
            _cameraServiceMock.Verify(service => service.AddCameraAsync(newCamera), Times.Once);
        }
    }

}
