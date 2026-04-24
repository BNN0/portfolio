using Luxottica.ApplicationServices.DivertLines;
using Luxottica.ApplicationServices.MapDivert;
using Luxottica.ApplicationServices.PhysicalMaps;
using Luxottica.ApplicationServices.Shared.Dto.DivertLines;
using Luxottica.ApplicationServices.Shared.Dto.MapDivert;
using Luxottica.ApplicationServices.Shared.Dto.MapPhysicVirtualSAP;
using Luxottica.Controllers;
using Luxottica.Controllers.DivertOutboundLines;
using Luxottica.Controllers.Totes.Luxottica.Controllers.PhysicalMaps;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.UnitTest.MapDivertControllerTest
{

        [TestFixture]
        public class MapDivertControllerTests
        {
            private MapDivertController _controller;
            private Mock<IMapDivertAppService> _mapDivertAppService;
            private Mock<ILogger<MapDivertController>> _loggerMapDivertMock;
        [SetUp]
            public void Setup()
            {
            _mapDivertAppService = new Mock<IMapDivertAppService>();
            _loggerMapDivertMock = new Mock<ILogger<MapDivertController>>();
            _controller = new MapDivertController(_mapDivertAppService.Object, _loggerMapDivertMock.Object);
            }

            [Test]
            public async Task GetList_ValidData_ReturnsOkResult()
            {
                // Arrange
                var vector = new VectorModel { Values = new List<int> { 1, 2, 3 } };
                _mapDivertAppService.Setup(x => x.AssignVirtualZones(It.IsAny<int>(), It.IsAny<VectorModel>())).ReturnsAsync(true);

                // Act
                var result = await _controller.GetList(1, vector);

                // Assert
                Assert.IsInstanceOf<OkResult>(result);
            }

            [Test]
            public async Task GetList_InvalidData_ReturnsBadRequestResult()
            {
                // Arrange
                var vector = new VectorModel { Values = null };

                // Act
                var result = await _controller.GetList(1, vector) as ObjectResult;

                // Assert;
                Assert.AreEqual(500, result.StatusCode);
        }

            [Test]
            public async Task DisAssign_ValidId_ReturnsOkResult()
            {
                // Arrange
                _mapDivertAppService.Setup(x => x.DisAssignVirtualZones(It.IsAny<int>())).ReturnsAsync(true);

                // Act
                var result = await _controller.DisAssign(1);

                // Assert
                Assert.IsInstanceOf<OkResult>(result);
            }

            [Test]
            public async Task DisAssign_InvalidId_ReturnsBadRequestResult()
            {
                // Act
                var result = await _controller.DisAssign(0) as ObjectResult;

                // Assert
                Assert.AreEqual(500, result.StatusCode);
        }
        }
    }

