using Luxottica.ApplicationServices.DivertLines;
using Luxottica.ApplicationServices.PhysicalMaps;
using Luxottica.ApplicationServices.Shared.Dto.MapPhysicVirtualSAP;
using Luxottica.Controllers.DivertLines;
using Luxottica.Controllers.PhysicalMaps;
using Luxottica.Controllers.SeconLevelCameraAssignment;
using Luxottica.Core.Entities.PhysicalMaps;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Luxottica.UnitTest
{
    public class MapPhysicalVirtualSAPTest
    {
        private Mock<IMapPhysicalAppService> _mapPhysicalServiceMock;
        private MapPhysicalVirualSAPController _controller;

        [SetUp]
        public void Setup()
        {
            var loggerMock = new Mock<ILogger<MapPhysicalVirualSAPController>>();
            _mapPhysicalServiceMock = new Mock<IMapPhysicalAppService>();
            _controller = new MapPhysicalVirualSAPController(_mapPhysicalServiceMock.Object, loggerMock.Object);

        }

        [Test]
        public async Task TestAdd()
        {
            // Arrange
            int divertLineId = 1;
            int virtualSAPZoneId = 10;
            var newMapPhysical = new MapPhysicVirtualSAPAddDto { DivertLineId = divertLineId, VirtualSAPZoneId = virtualSAPZoneId };

            _mapPhysicalServiceMock.Setup(service => service.AddMapPhysicalAsync(newMapPhysical));

            // Act
            await _controller.Post(newMapPhysical);

            // Assert
            _mapPhysicalServiceMock.Verify(service => service.AddMapPhysicalAsync(newMapPhysical), Times.Once);
        }

        [Test]
        public async Task TestGetAll()
        {
            // Arrange
            var expectedMapPhysical = new List<MapPhysicVirtualSAPDto>
            {
                new MapPhysicVirtualSAPDto{ Id = 1, DivertLineId = 1, VirtualSAPZoneId = 1},
                new MapPhysicVirtualSAPDto{ Id = 2, DivertLineId = 2, VirtualSAPZoneId = 2}
            };
            _mapPhysicalServiceMock.Setup(service => service.GetMapPhysicalAsync())
                .ReturnsAsync(expectedMapPhysical);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var mapsPhysical = result as ObjectResult;
            Assert.NotNull(mapsPhysical);
            Assert.AreEqual(StatusCodes.Status200OK, mapsPhysical.StatusCode);

            var mapsPhysicals = mapsPhysical.Value as List<MapPhysicVirtualSAPDto>;
            Assert.NotNull(mapsPhysicals);
            Assert.AreEqual(expectedMapPhysical.Count, mapsPhysicals.Count);
        }

        [Test]
        public async Task TestGetById()
        {
            // Arrange
            int testId = 1;
            int diverlineId = 1;
            int mapVirtualZoneId = 1;
            _mapPhysicalServiceMock.Setup(service => service.GetMapPhysicalByIdAsync(testId))
                .ReturnsAsync(new MapPhysicVirtualSAPDto { Id = testId, DivertLineId = diverlineId, VirtualSAPZoneId = mapVirtualZoneId });

            // Act
            var result = await _controller.GetById(testId);

            // Assert
            var mapPhysical = result as ObjectResult;
            Assert.NotNull(mapPhysical);
            Assert.AreEqual(StatusCodes.Status200OK, mapPhysical.StatusCode);

            var mapsphysicals = mapPhysical.Value as MapPhysicVirtualSAPDto;
            Assert.NotNull(mapsphysicals);
            Assert.AreEqual(testId, mapsphysicals.Id);
        }
        [Test]
        public async Task TestEdit()
        {
            // Arrange
            int testId = 1;
            var editedmapPhysical = new MapPhysicVirtualSAPAddDto { DivertLineId = 1, VirtualSAPZoneId = 10 };
            _mapPhysicalServiceMock.Setup(service => service.EditMapPhysicalAsync(testId, editedmapPhysical));

            // Act
            await _controller.Put(testId, editedmapPhysical);

            // Assert
            _mapPhysicalServiceMock.Verify(service => service.EditMapPhysicalAsync(testId, editedmapPhysical), Times.Once);
        }

        [Test]
        public async Task TestDelete()
        {
            // Arrange
            int testId = 1;
            _mapPhysicalServiceMock.Setup(service => service.DeleteMapPhysicalAsync(testId));

            // Act
            await _controller.Delete(testId);

            // Assert
            _mapPhysicalServiceMock.Verify(service => service.DeleteMapPhysicalAsync(testId), Times.Once);
        }
    }
}