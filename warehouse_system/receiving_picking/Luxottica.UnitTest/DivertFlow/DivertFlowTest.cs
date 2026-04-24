using Luxottica.ApplicationServices.Scanlogs;
using Luxottica.ApplicationServices.ToteInformations;
using Luxottica.Controllers.Commissioners;
using Luxottica.Controllers.DivertFlow;
using Luxottica.Models.DivertFlow;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Luxottica.UnitTest.DivertFlow
{
    [TestFixture]
    public class DivertFlowControllerTest
    {
        private Mock<IToteInformationAppService> _toteServiceMock;

        private DivertFlowController _controller;
        private Mock<ILogger<DivertFlowController>> _loggerMock;
        private Mock<IScanlogsAppService> _scanlogsAppService;

        [SetUp]
        public void Setup()
        {
            _toteServiceMock = new Mock<IToteInformationAppService>();
            _loggerMock = new Mock<ILogger<DivertFlowController>>();
            _scanlogsAppService = new Mock<IScanlogsAppService>();
            _controller = new DivertFlowController(_toteServiceMock.Object, _loggerMock.Object, _scanlogsAppService.Object);
        }

        [Test]
        public async Task Confirmation()
        {
            // Arrange
            var confirmationModel = new ConfirmationModel
            {
                camId = "Cam06",
                trakingId = 1,
                divertCode = 100
            };

            _toteServiceMock.Setup(service => service.DivertConfirm(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(2);

            // Act
            var result = await _controller.Confirmation(confirmationModel);

            // Assert
            Assert.IsInstanceOf<OkResult>(result);
        }

        [Test]
        public async Task ConfirmationCam14()
        {
            // Arrange
            var confirmationModel = new ConfirmationModel
            {
                camId = "Cam14",
                trakingId = 1,
                divertCode = 100
            };

            _toteServiceMock.Setup(service => service.DivertConfirmCam14(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(2);

            // Act
            var result = await _controller.ConfirmationCam14(confirmationModel);

            // Assert
            Assert.IsInstanceOf<OkResult>(result);
        }

        [Test]
        public async Task ConfirmationCam15()
        {
            // Arrange
            var confirmationModel = new ConfirmationModel
            {
                camId = "Cam15",
                trakingId = 1,
                divertCode = 100
            };

            _toteServiceMock.Setup(service => service.DivertConfirmCam15(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(2);

            // Act
            var result = await _controller.ConfirmationCam15(confirmationModel);

            // Assert
            Assert.IsInstanceOf<OkResult>(result);
        }

        [Test]
        public async Task ConfirmationCam16()
        {
            // Arrange
            var confirmationModel = new ConfirmationModel
            {
                camId = "Cam16",
                trakingId = 1,
                divertCode = 100
            };

            _toteServiceMock.Setup(service => service.DivertConfirmCam16(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(2);

            // Act
            var result = await _controller.ConfirmationCam16(confirmationModel);

            // Assert
            Assert.IsInstanceOf<OkResult>(result);
        }

        [Test]
        public async Task ConfirmationCam17()
        {
            // Arrange
            var confirmationModel = new ConfirmationModel
            {
                camId = "Cam17",
                trakingId = 1,
                divertCode = 100
            };

            _toteServiceMock.Setup(service => service.DivertConfirmCam17(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(2);

            // Act
            var result = await _controller.ConfirmationCam17(confirmationModel);

            // Assert
            Assert.IsInstanceOf<OkResult>(result);
        }
    }
}
