using LuxotticaSorting.ApplicationServices.RoutingV10;
using LuxotticaSorting.ApplicationServices.Shared.Dto.RoutingV10S;
using LuxotticaSorting.Controllers.RoutingV10S;
using LuxotticaSorting.Core.WCSRoutingV10;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LuxxoticaSorting.Unitest.RoutingV10STest
{
    public class RoutingV10sTest
    {
        private Mock<ILogger<RoutingV10SController>> _loggerRoutingMock;
        private Mock<IRoutingAppService> _routingServiceMock;
        private RoutingV10SController _routingInfoController;

        [SetUp]
        public void Setup()
        {
            _loggerRoutingMock = new Mock<ILogger<RoutingV10SController>>();
            _routingServiceMock = new Mock<IRoutingAppService>();
            _routingInfoController = new RoutingV10SController(_routingServiceMock.Object, _loggerRoutingMock.Object);
        }

        [Test]
        public async Task GetAllBoxInfo()
        {
            // Arrange
            var BoxesInformation = new List<RoutingV10Dto> 
            {
               new  RoutingV10Dto {Id = 1, BoxId = "C01", BoxType = "CH", CarrierCode = "UPSGC", LogisticAgent = "Z001", ContainerType = "G", 
                   CurrentTs = DateTime.Now.ToString("yyyyMMddHHmmssfff"), ContainerId = "GLD0001",ConfirmationNumber = "C1223331", DivertLane = 4,
                   Qty = 2, SAPSystem = "FSD1", Status = "NULL" },
               new  RoutingV10Dto {Id = 2, BoxId = "C02", BoxType = "MD", CarrierCode = "UPSGC", LogisticAgent = "Z002", ContainerType = "T", 
                   CurrentTs = DateTime.Now.ToString("yyyyMMddHHmmssfff"), ContainerId = "GLD0001",ConfirmationNumber = "C1223331", DivertLane = 4, 
                   Qty = 2, SAPSystem = "FSD1", Status = "NULL" }
            };

            _routingServiceMock.Setup(service => service.GetBoxesInformationRoutingAsync())
                .ReturnsAsync(BoxesInformation);

            // Act
            var result = await _routingInfoController.GetAll();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

            var boxes = okResult.Value as List<RoutingV10Dto>;
            Assert.NotNull(boxes);
            Assert.AreEqual(2, boxes.Count);
        }

        [Test]
        public async Task GetByBoxIdInSAPInformation()
        {
            // Arrange
            var boxInformationSAP = new WCSRoutingV10
            {
                Id = 1,
                BoxId = "C01",
                BoxType = "CH",
                CarrierCode = "UPSGC",
                ContainerType = "G",
                CurrentTs = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                LogisticAgent = "Z001",
                ContainerId = "GLD0001",
                ConfirmationNumber = "C1223331",
                DivertLane = 4,
                Qty = 2,
                SAPSystem = "FSD1",
                Status = "NULL"
            };

            string searchBoxId = "C01";
            int trackingId = 1;

            _routingServiceMock.Setup(service => service.GetOrdersBoxIdSAPInformation(searchBoxId, trackingId))
                .ReturnsAsync(new RoutingV10Dto { Id = 1, BoxId = boxInformationSAP.BoxId, BoxType = boxInformationSAP.BoxType, 
                    CarrierCode = boxInformationSAP.CarrierCode, ContainerType =  boxInformationSAP.ContainerType,
                    LogisticAgent = boxInformationSAP.LogisticAgent, CurrentTs = boxInformationSAP.CurrentTs, 
                    ConfirmationNumber = boxInformationSAP.ConfirmationNumber, ContainerId = boxInformationSAP.ContainerId, 
                    DivertLane = boxInformationSAP.DivertLane, SAPSystem = boxInformationSAP.SAPSystem, 
                    Qty = boxInformationSAP.Qty, Status = boxInformationSAP.Status} );

            // Act
            var result = await _routingInfoController.GetById(searchBoxId, trackingId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        }
    }
}
