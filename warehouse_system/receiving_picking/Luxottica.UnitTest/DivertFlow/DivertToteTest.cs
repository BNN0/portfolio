using Luxottica.ApplicationServices.Scanlogs;
using Luxottica.ApplicationServices.ToteInformations;
using Luxottica.Controllers.DivertFlow;
using Luxottica.Models.DivertFlow;
using Luxottica.Models.TransferInboud;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Luxottica.UnitTest.DivertFlow
{
    public class DivertToteTest
    {
        private Mock<IToteInformationAppService> _toteInformationAppServiceMock;
        private DivertFlowController _toteFlowControllerMock;
        private Mock<ILogger<DivertFlowController>> _loggerMock;
        private Mock<IScanlogsAppService> _scanlogsAppService;
        [SetUp]
        public void Setup()
        {
            _toteInformationAppServiceMock = new Mock<IToteInformationAppService>();
            _loggerMock = new Mock<ILogger<DivertFlowController>>();
            _scanlogsAppService = new Mock<IScanlogsAppService>();
            _toteFlowControllerMock = new DivertFlowController(_toteInformationAppServiceMock.Object,_loggerMock.Object, _scanlogsAppService.Object);
        }

        [Test]
        public async Task DivertTransfer01AND02()
        {
            // Arrange
            var divertModel = new DivertModel
            {
                camId = "CAM04",
                trakingId = 1,
                toteLpn = "K000006",
                scannerNLaneWStatus = 1,
                scannerNLaneWFull = 0
            };

            _toteInformationAppServiceMock.Setup(x => x.DivertTote(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                           .ReturnsAsync(2);

            // Act
            var result = await _toteFlowControllerMock.Divert(divertModel);

            // Assert
            var okResult = result as OkObjectResult;

            Assert.IsNotNull(okResult);
            Assert.AreEqual(2, okResult.Value.GetType().GetProperty("divert_code").GetValue(okResult.Value));

        }
        [Test]
        public async Task DivertCam12()
        {
            // Arrange
            var divertModel = new DivertModel
            {
                camId = "CAM12",
                trakingId = 112,
                toteLpn = "TLPN00001",
                scannerNLaneWStatus = 1,
                scannerNLaneWFull = 0
            };

            _toteInformationAppServiceMock.Setup(x => x.DivertTote(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                           .ReturnsAsync(2);

            // Act
            var result = await _toteFlowControllerMock.Divert(divertModel);

            // Assert
            var okResult = result as OkObjectResult;

            Assert.IsNotNull(okResult);
            Assert.AreEqual(2, okResult.Value.GetType().GetProperty("divert_code").GetValue(okResult.Value));

        }
        [Test]
        public async Task DivertCam13()
        {
            // Arrange
            var divertModel = new DivertModel
            {
                camId = "CAM13",
                trakingId = 113,
                toteLpn = "TLPN00002",
                scannerNLaneWStatus = 1,
                scannerNLaneWFull = 0
            };

            _toteInformationAppServiceMock.Setup(x => x.DivertTote(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                           .ReturnsAsync(2);

            // Act
            var result = await _toteFlowControllerMock.Divert(divertModel);

            // Assert
            var okResult = result as OkObjectResult;

            Assert.IsNotNull(okResult);
            Assert.AreEqual(2, okResult.Value.GetType().GetProperty("divert_code").GetValue(okResult.Value));

        }
        [Test]
        public async Task DivertCam14()
        {
            // Arrange
            var divertModel = new DivertModel
            {
                camId = "CAM14",
                trakingId = 114,
                toteLpn = "TLPN00003",
                scannerNLaneWStatus = 1,
                scannerNLaneWFull = 0
            };

            _toteInformationAppServiceMock.Setup(x => x.DivertTote(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                           .ReturnsAsync(2);

            // Act
            var result = await _toteFlowControllerMock.Divert(divertModel);

            // Assert
            var okResult = result as OkObjectResult;

            Assert.IsNotNull(okResult);
            Assert.AreEqual(2, okResult.Value.GetType().GetProperty("divert_code").GetValue(okResult.Value));

        }
        [Test]
        public async Task DivertCam15()
        {
            // Arrange
            var divertModel = new DivertModel
            {
                camId = "CAM15",
                trakingId = 115,
                toteLpn = "TLPN00005",
                scannerNLaneWStatus = 1,
                scannerNLaneWFull = 0
            };

            _toteInformationAppServiceMock.Setup(x => x.DivertTote(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                           .ReturnsAsync(2);

            // Act
            var result = await _toteFlowControllerMock.Divert(divertModel);

            // Assert
            var okResult = result as OkObjectResult;

            Assert.IsNotNull(okResult);
            Assert.AreEqual(2, okResult.Value.GetType().GetProperty("divert_code").GetValue(okResult.Value));

        }
        [Test]
        public async Task DivertCam16()
        {
            // Arrange
            var divertModel = new DivertModel
            {
                camId = "CAM16",
                trakingId = 116,
                toteLpn = "TLPN00006",
                scannerNLaneWStatus = 1,
                scannerNLaneWFull = 0
            };

            _toteInformationAppServiceMock.Setup(x => x.DivertTote(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                           .ReturnsAsync(2);

            // Act
            var result = await _toteFlowControllerMock.Divert(divertModel);

            // Assert
            var okResult = result as OkObjectResult;

            Assert.IsNotNull(okResult);
            Assert.AreEqual(2, okResult.Value.GetType().GetProperty("divert_code").GetValue(okResult.Value));

        }
        [Test]
        public async Task DivertCam17()
        {
            // Arrange
            var divertModel = new DivertModel
            {
                camId = "CAM17",
                trakingId = 117,
                toteLpn = "TLPN00007",
                scannerNLaneWStatus = 1,
                scannerNLaneWFull = 0
            };

            _toteInformationAppServiceMock.Setup(x => x.DivertTote(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                           .ReturnsAsync(2);

            // Act
            var result = await _toteFlowControllerMock.Divert(divertModel);

            // Assert
            var okResult = result as OkObjectResult;

            Assert.IsNotNull(okResult);
            Assert.AreEqual(2, okResult.Value.GetType().GetProperty("divert_code").GetValue(okResult.Value));

        }
    }
}
