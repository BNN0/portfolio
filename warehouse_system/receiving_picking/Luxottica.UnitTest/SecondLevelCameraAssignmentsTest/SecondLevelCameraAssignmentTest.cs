using Luxottica.ApplicationServices.SecondLevelCameraAssignments;
using Luxottica.ApplicationServices.Shared.Dto.SecondLevelCamera;
using Luxottica.Controllers.SeconLevelCameraAssignment;
using Luxottica.Controllers.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Luxottica.UnitTest.SecondLevelCameraAssignmentsTest
{
    public class SecondLevelCameraAssignmentTest
    {
        private Mock<ISecondLevelCameraAppService> _secondLevelServiceMock;
        private SecondLevelCameraController _secondLevelCameraControllerMock;
 
        [SetUp]
        public void Setup()
        {
            _secondLevelServiceMock = new Mock<ISecondLevelCameraAppService>();
            var loggerMock = new Mock<ILogger<SecondLevelCameraController>>();
            _secondLevelCameraControllerMock = new SecondLevelCameraController(_secondLevelServiceMock.Object, loggerMock.Object);
        }

        [Test]
        public async Task GetInfoSecondLevelCA()
        {
            //Arrange
            var expectedSecondLevelCameraAssignment = new SecondLevelCameraBundleDto { CameraId = 1, CameraName = "cam01" };
            _secondLevelServiceMock.Setup(service => service.GetCameraAssignmentInfo())
                .ReturnsAsync(expectedSecondLevelCameraAssignment);
            //Act
            var result = await _secondLevelCameraControllerMock.GetCameraAssignmentInfo();

            //Assert
            Assert.NotNull(result);
            var res = result as SecondLevelCameraBundleDto;
            Assert.NotNull(res);
            Assert.AreEqual(1,res.CameraId);
        }

        [Test]
        public async Task GetAsyncSecondLevelCA()
        {
            //Arrange
            var expectedGetAsyncSLCA = new SecondLevelCameraGetDto { CameraId = 1, Id = 1 };
            _secondLevelServiceMock.Setup(service => service.GetSecondLevelCameraAsync())
                .ReturnsAsync(expectedGetAsyncSLCA);
            //Act
            var result = await _secondLevelCameraControllerMock.Get();
            //Assert
            Assert.NotNull(result);
            var res = result as SecondLevelCameraGetDto;
            Assert.NotNull(res);
            Assert.AreEqual(1, res.Id);
        }

        [Test]
        public async Task ChangeSecondLvCA()
        {
            //Arrange
            int cameraId = 1;
            _secondLevelServiceMock.Setup(service => service.ChangeSecondLevelCamera(cameraId)).ReturnsAsync(true);
            //Act
            var result = await _secondLevelCameraControllerMock.ChangeCameraAssignment(cameraId);
            //Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Test]
        public async Task Delete()
        {
            //Arrange
            int testId = 1;
            _secondLevelServiceMock.Setup(service => service.DeleteSecondLevelCamerasAsync(testId));
            //Act
            await _secondLevelCameraControllerMock.Delete(testId);
            //Assert
            _secondLevelServiceMock.Verify(service => service.DeleteSecondLevelCamerasAsync(testId), Times.Once);
        }
    }
}
