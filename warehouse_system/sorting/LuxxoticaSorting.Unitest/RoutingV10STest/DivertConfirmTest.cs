using AutoMapper;
using LuxotticaSorting.ApplicationServices.DivertBox;
using LuxotticaSorting.ApplicationServices.DivertLanes;
using LuxotticaSorting.ApplicationServices.RoutingV10;
using LuxotticaSorting.ApplicationServices.Shared.DTO.RoutingV10S;
using LuxotticaSorting.Controllers.DivertBox;
using LuxotticaSorting.Controllers.RoutingV10S;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LuxxoticaSorting.Unitest.RoutingV10STest
{
    public class DivertConfirmTest
    {
        private DivertBoxController _controller;
        private Mock<IDivertBoxAppService> _divertBoxAppService;
        private Mock<ILogger<DivertBoxController>> _logger;
        private Mock<RoutingV10SController> _routingV10SController;
        private Mock<IDivertLanesAppService> _divertLanesAppService;
        private Mock<IRoutingAppService> _routingAppService;

        [SetUp]
        public void Setup()
        {
            _divertBoxAppService = new Mock<IDivertBoxAppService>();
            _divertLanesAppService = new Mock<IDivertLanesAppService>();
            _logger = new Mock<ILogger<DivertBoxController>>();
            _routingAppService = new Mock<IRoutingAppService>();
            _routingV10SController = new Mock<RoutingV10SController>();
            _controller = new DivertBoxController(_divertBoxAppService.Object, _logger.Object, _divertLanesAppService.Object, _routingAppService.Object);
        }

        [Test]
        public async Task Confirmation()
        {
            // Arrange
            var confirmationDto = new ConfirmBoxReqDto
            {
                DivertCode = 100,
                TrackingId = 1
            };

            _divertBoxAppService.Setup(s => s.DivertConfirm(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(true);

            // Act
            var result = await _controller.Confirmation(confirmationDto) as OkResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
        }
    }
}
