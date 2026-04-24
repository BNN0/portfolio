using LuxotticaSorting.ApplicationServices.Mapping.BoxTypeDivertLane;
using LuxotticaSorting.ApplicationServices.Shared.DTO.BoxTypeDivertLaneMapping;
using LuxotticaSorting.Controllers.Mapping.BoxType_DivertLane;
using LuxotticaSorting.Core.Mapping.BoxTypeDivertLane;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxxoticaSorting.Unitest.BoxTypeDivertLaneMappingsTest
{
    public class BoxTypeDivertLaneMappingTest
    {
        [Test]
        public async Task GetAll()
        {
            // Arrange
            var boxTypeDivertLaneAppServiceMock = new Mock<IBoxTypeDivertLaneAppService>();
            var loggerMock = new Mock<ILogger<BoxTypeDivertLaneMappingController>>();

            var controller = new BoxTypeDivertLaneMappingController(boxTypeDivertLaneAppServiceMock.Object, loggerMock.Object);

            var expectedMappings = new List<BoxTypeDivertLaneMapping>
            {
                new BoxTypeDivertLaneMapping { Id = 1, BoxTypeId = 1, DivertLaneId = 2 },
                new BoxTypeDivertLaneMapping { Id = 2, BoxTypeId = 3, DivertLaneId = 4 }
            };

            boxTypeDivertLaneAppServiceMock.Setup(service => service.GetAllBoxTypeDivertLaneMappingsAsync())
                .ReturnsAsync(expectedMappings);

            // Act
            var result = await controller.GetAll();

            // Assert  
            var resultMappings = result as List<BoxTypeDivertLaneMapping>;
            Assert.IsNotNull(result);
            Assert.That(result.Count(), Is.EqualTo(expectedMappings.Count));
        }

        [Test]
        public async Task GetById()
        {
            // Arrange
            int itemId = 1;
            var expectedMapping = new BoxTypeDivertLaneMapping { Id = itemId, BoxTypeId = 1, DivertLaneId = 2 };

            var boxTypeDivertLaneAppServiceMock = new Mock<IBoxTypeDivertLaneAppService>();
            boxTypeDivertLaneAppServiceMock.Setup(service => service.GetBoxTypeDivertLaneMappingAsync(itemId))
                .ReturnsAsync(expectedMapping);

            var loggerMock = new Mock<ILogger<BoxTypeDivertLaneMappingController>>();
            var controller = new BoxTypeDivertLaneMappingController(boxTypeDivertLaneAppServiceMock.Object, loggerMock.Object);

            // Act
            var result = await controller.GetById(itemId);

            // Assert 
            Assert.IsNotNull(result);
            Assert.That(result.Id, Is.EqualTo(itemId));
        }

        [Test]
        public async Task Post()
        {
            // Arrange
            var boxTypeDivertLaneAppServiceMock = new Mock<IBoxTypeDivertLaneAppService>();
            var loggerMock = new Mock<ILogger<BoxTypeDivertLaneMappingController>>();
            var controller = new BoxTypeDivertLaneMappingController(boxTypeDivertLaneAppServiceMock.Object, loggerMock.Object);
            var model = new BoxTypeDivertLaneMappingAddDto { BoxTypeId = 1, DivertLaneId = 2 };

            // Act
            var result = await controller.Post(model);

            // Assert 
            Assert.IsNotNull(result); 
        }

        [Test]
        public async Task Put()
        {
            // Arrange
            int itemId = 1;
            var boxTypeDivertLaneAppServiceMock = new Mock<IBoxTypeDivertLaneAppService>();
            var loggerMock = new Mock<ILogger<BoxTypeDivertLaneMappingController>>();
            var controller = new BoxTypeDivertLaneMappingController(boxTypeDivertLaneAppServiceMock.Object, loggerMock.Object);
            var model = new BoxTypeDivertLaneMappingAddDto { BoxTypeId = 1, DivertLaneId = 2 };

            // Act
            var result = await controller.Put(itemId, model);

            // Assert 
            Assert.IsNotNull(result); 
        }

        [Test]
        public async Task Delete()
        {
            // Arrange
            int itemId = 1;
            var boxTypeDivertLaneAppServiceMock = new Mock<IBoxTypeDivertLaneAppService>();
            var loggerMock = new Mock<ILogger<BoxTypeDivertLaneMappingController>>();
            var controller = new BoxTypeDivertLaneMappingController(boxTypeDivertLaneAppServiceMock.Object, loggerMock.Object);

            // Act
            await controller.Delete(itemId);

            // Assert
            // You can assert that the Delete method was called with the correct ID, depending on your implementation.
            boxTypeDivertLaneAppServiceMock.Verify(service => service.DeleteBoxTypeDivertLaneMappingAsync(itemId), Times.Once);
        }

    }
}
