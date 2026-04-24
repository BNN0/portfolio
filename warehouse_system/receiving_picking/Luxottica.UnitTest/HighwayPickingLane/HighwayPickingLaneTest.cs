using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Luxottica.Controllers.HighwayPickingLanes;
using Luxottica.ApplicationServices.HighwayPickingLanes;
using Luxottica.ApplicationServices.Shared.Dto.HighwayPickingLanes;
using Luxottica.Models.HighwayPickingLanes;
using Luxottica.Controllers.LimitSettings;

namespace Luxottica.UnitTest.HighwayPickingLane
{
    public class HighwayPickingLaneTest
    {
        private Mock<IHighWayPickingLanesAppService> _highwayPickingService;
        private HighwayPickingLanesController _highwaycontroller;

        [SetUp]
        public void Setup()
        {
            _highwayPickingService = new Mock<IHighWayPickingLanesAppService>();
            var loggerMock = new Mock<ILogger<HighwayPickingLanesController>>();
            _highwaycontroller = new HighwayPickingLanesController(_highwayPickingService.Object, loggerMock.Object);
        }
        [Test]
        public async Task GetLimitsHighway_ReturnsOkResultWithLimits()
        {
            // Arrange
            var mockLimits = new HighwayPickingLanesDTO
            {
                MultiTotes = 10,
                MaxTotesLPTUMachine1 = 5,
                MaxTotesSPTAMachine1 = 15,
                MaxTotesSPTAMachine2 = 20,
            };

            _highwayPickingService.Setup(s => s.GetHighwayPickingLaneAsync()).ReturnsAsync(new List<HighwayPickingLanesDTO> { mockLimits });

            // Act
            var result = await _highwaycontroller.GetLimitsHighway();

            // Assert
            var okResult = result as ActionResult<object>;
            Assert.IsNotNull(okResult);
        }


        [Test]
        public async Task EditLimits_ReturnsOkResult()
        {
            var request = new HighwayPickingRequest
            {
                LimitHighway = 20,
                LimitLPTUMachine1 = 60,
                LimitSPTAMachine1 = 60,
                LimitSPTAMachine2 = 60,
            };
            _highwayPickingService.Setup(s => s.GetHighwayPickingLaneAsync()).ReturnsAsync(new List<HighwayPickingLanesDTO>());

            // Act
            var result = await _highwaycontroller.EditLimits(request);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var response = okResult.Value as object; 

        }


    }
}
