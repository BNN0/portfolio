using Luxottica.ApplicationServices.DivertOutboundLines;
using Luxottica.ApplicationServices.Shared.Dto.DivertOutboundLines;
using Luxottica.Controllers.DivertOutboundLines;
using Luxottica.Controllers.LimitSettings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.UnitTest.DivertOutboundLinesTest
{
    [TestFixture]
    public class DivertOutboundLine
    {
        private Mock<IDivertOutboundLine> _divertOutboundLineService;
        private DivertOutboundLineController _divertOutboundLineController;
        private Mock<ILogger<DivertOutboundLineController>> _loggerMock;
        [SetUp]
        public void Setup()
        {
            _divertOutboundLineService = new Mock<IDivertOutboundLine>();
            _loggerMock = new Mock<ILogger<DivertOutboundLineController>>();
            _divertOutboundLineController = new DivertOutboundLineController(_divertOutboundLineService.Object, _loggerMock.Object);
        }

        [Test]
        public async Task GetDivertOutboundLine_ReturnsOkResultWithLimits()
        {
            // Arrange
            var mockLimits = new DivertOutboundLineDTO
            {
                MultiTotes = 10,
                MaxTotesLPTUMachine1 = 5,
                MaxTotesSPTAMachine1 = 15,
                MaxTotesSPTAMachine2 = 20,
            };

            _divertOutboundLineService.Setup(s => s.GetDivertOutboundLineAsync()).ReturnsAsync(new List<DivertOutboundLineDTO> { mockLimits });

            // Act
            var result = await _divertOutboundLineController.GetLimitsDivertOutboundLine();

            // Assert
            var okResult = result as ActionResult<object>;
            Assert.IsNotNull(okResult);
        }

        [Test]
        public async Task GetDivertOutboundLinePresent_ReturnsOkResultWithLimits()
        {
            // Arrange
            var mockLimits = new DivertOutboundLineDTO
            {
                MultiTotes = 10,
                MaxTotesLPTUMachine1 = 5,
                MaxTotesSPTAMachine1 = 15,
                MaxTotesSPTAMachine2 = 20,
            };

            _divertOutboundLineService.Setup(s => s.GetDivertOutboundLineAsync()).ReturnsAsync(new List<DivertOutboundLineDTO> { mockLimits });

            // Act
            var result = await _divertOutboundLineController.GetLimitsDivertOutboundLinePresent();

            // Assert
            var okResult = result as ActionResult<object>;
            Assert.IsNotNull(okResult);
        }


        [Test]
        public async Task EditLimits_ReturnsOkResult()
        {
            var request = new DivertOutboundLineRequestDto
            {
                LimitDivertOutbound = 12,
                LimitLPTUMachine1 = 36,
                LimitSPTAMachine1 = 36,
                LimitSPTAMachine2 = 36,
            };
            _divertOutboundLineService.Setup(s => s.GetDivertOutboundLineAsync()).ReturnsAsync(new List<DivertOutboundLineDTO>());

            // Act
            var result = await _divertOutboundLineController.EditLimits(request);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var response = okResult.Value as object;
        }

        [Test]
        public async Task EditLimitsPresent_ReturnsOkResult()
        {
            var request = new DivertOutboundLineRequestDto
            {
                LimitDivertOutbound = 12,
                LimitLPTUMachine1 = 36,
                LimitSPTAMachine1 = 36,
                LimitSPTAMachine2 = 36,
            };
            _divertOutboundLineService.Setup(s => s.GetDivertOutboundLineAsync()).ReturnsAsync(new List<DivertOutboundLineDTO>());

            // Act
            var result = await _divertOutboundLineController.UpdateLimitsDivertOutboundLinePresent(request);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var response = okResult.Value as object;
        }


    }
}
