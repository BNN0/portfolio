using Luxottica.ApplicationServices.Cameras;
using Luxottica.ApplicationServices.Shared.Dto.Camera;
using Luxottica.ApplicationServices.ToteInformations;
using Luxottica.Controllers.CameraAssignments;
using Luxottica.Controllers.Cameras;
using Luxottica.Controllers.Totes;
using Luxottica.Core.Entities.CameraAssignments;
using Luxottica.Core.Entities.Cameras;
using Luxottica.Core.Entities.ToteInformations;
using Luxottica.Models.NewTote;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.UnitTest.CamerasTest
{
    public class ApiCameraTest
    {
        private Mock<ICameraAppService> _cameraServiceMock;
        private CameraController _controller;
        private Mock<ILogger<CameraController>> _loggerMock;
        [SetUp]
        public void Setup()
        {
            _cameraServiceMock = new Mock<ICameraAppService>();
            _loggerMock = new Mock<ILogger<CameraController>>();
            _controller = new CameraController(_cameraServiceMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task TestGetAll()
        {
            // Arrange
            _cameraServiceMock.Setup(service => service.GetCamerasAsync())
                .ReturnsAsync(new List<CameraDTO>());
 
            // Act
            var result = await _controller.GetAll();

            List<Camera> cameras = new List<Camera>();
            foreach (var cameraDTO in result)
            {
                
                cameras.Add( new Camera()
                {
                    Id = cameraDTO.Id,
                    CamId = cameraDTO.CamId,
                });
            }
            

            // Assert
            var cam = cameras as List<Camera>;
            Assert.NotNull(cam);
            Assert.IsEmpty(cam);
        }

        [Test]
        public async Task TestGetById()
        {
            int camId = 1;
            _cameraServiceMock.Setup(service => service.GetCameraByIdAsync(camId))
                .ReturnsAsync(new CameraDTO
                {
                    Id = camId,
                    CamId = "cam10",
                });

            // Act
            var result = await _controller.GetById(camId);

            // Assert
            var cam = result as CameraDTO;
            Assert.NotNull(cam);
            Assert.AreEqual(camId, cam.Id);
        }
        [Test]
        public async Task TestEdit()
        {
            // Arrange
            int camId = 1;
            var camEdit = new CameraDTO
            {
                Id = camId,
                CamId = "CAM04",
            };
            _cameraServiceMock.Setup(service => service.EditCameraAsync(camEdit));

            // Act
            await _controller.Put(camId, camEdit);

            // Assert
            _cameraServiceMock.Verify(service => service.EditCameraAsync(camEdit), Times.Once);
        }

        [Test]
        public async Task TestDelete()
        {
            // Arrange
            int camId = 1;
            _cameraServiceMock.Setup(service => service.DeleteCameraAsync(camId));

            // Act
            await _controller.Delete(camId);

            // Assert
            _cameraServiceMock.Verify(service => service.DeleteCameraAsync(camId), Times.Once);
        }
        [Test]
        public async Task TestAdd()
        {
            // Arrange
            var camAssigmengt = new CameraAssignment { Id = 1, Camera = null, CameraId = 1, DivertLine = null, DivertLineId = 1 };
            var listCamAssigment = new List<CameraAssignment>() { camAssigmengt };
            var cameraObj = new Camera { Id = 1, CamId = "1string", CameraAssignments = listCamAssigment};

            var cam = new CameraDTO
            {
                CamId = cameraObj.CamId,
            };
            _cameraServiceMock.Setup(service => service.AddCameraAsync(cam));

            // Act
            var result = await _controller.Post(cam);

            // Assert
            Assert.NotNull(result);
            _cameraServiceMock.Verify(service => service.AddCameraAsync(cam), Times.Once);
        }
    }
}
