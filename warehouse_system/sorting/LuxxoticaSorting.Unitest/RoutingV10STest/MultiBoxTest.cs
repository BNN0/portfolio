using LuxotticaSorting.ApplicationServices.DivertBox;
using LuxotticaSorting.ApplicationServices.DivertLanes;
using LuxotticaSorting.ApplicationServices.RoutingV10;
using LuxotticaSorting.ApplicationServices.Shared.Dto.RoutingV10S;
using LuxotticaSorting.ApplicationServices.Shared.DTO.RoutingV10S;
using LuxotticaSorting.Controllers.DivertBox;
using LuxotticaSorting.Controllers.RoutingV10S;
using LuxotticaSorting.Core.WCSRoutingV10;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxxoticaSorting.Unitest.RoutingV10STest
{
    public class MultiBoxTest
    {
        private Mock<ILogger<RoutingV10SController>> _loggerRoutingMock;
        private Mock<IRoutingAppService> _routingServiceMock;
        private RoutingV10SController _routingInfoController;
        private Mock<RoutingV10SController> _routingController;
        private DivertBoxController _controller;
        private Mock<IDivertBoxAppService> _divertBoxAppService;
        private Mock<ILogger<DivertBoxController>> _logger;
        private RoutingV10SController _routingV10SController;
        private Mock<IDivertLanesAppService> _divertLanesAppService;

        [SetUp]
        public void Setup()
        {
            _loggerRoutingMock = new Mock<ILogger<RoutingV10SController>>();
            _routingServiceMock = new Mock<IRoutingAppService>();
            _routingInfoController = new RoutingV10SController(_routingServiceMock.Object, _loggerRoutingMock.Object);

            _divertBoxAppService = new Mock<IDivertBoxAppService>();
            _divertLanesAppService = new Mock<IDivertLanesAppService>();
            _logger = new Mock<ILogger<DivertBoxController>>();
            _controller = new DivertBoxController(_divertBoxAppService.Object, _logger.Object, _divertLanesAppService.Object, _routingServiceMock.Object);
        }


        
        [Test]
        public async Task GetByIdMultibox()
        {
        
            var boxInformationSAP = new WCSRoutingV10
            {
                Id = 1,
                BoxId = "C1006881659",
                BoxType = "IBOX_D2",
                CarrierCode = "UPSGC",
                ContainerType = "T",
                CurrentTs = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                LogisticAgent = "Z023",
                ContainerId = "1000001179",
                ConfirmationNumber = "C1006881670",
                DivertLane = 2,
                Qty = 2,
                SAPSystem = "AFS1",
                Status = "NA"
            };

            string searchBoxId = "C1006881659";
            int trackingId = 1;

            _routingServiceMock.Setup(service => service.GetOrdersBoxIdSAPInformation(searchBoxId, trackingId))
                .ReturnsAsync(new RoutingV10Dto
                {
                    Id = 1,
                    BoxId = boxInformationSAP.BoxId,
                    BoxType = boxInformationSAP.BoxType,
                    CarrierCode = boxInformationSAP.CarrierCode,
                    ContainerType = boxInformationSAP.ContainerType,
                    LogisticAgent = boxInformationSAP.LogisticAgent,
                    CurrentTs = boxInformationSAP.CurrentTs,
                    ConfirmationNumber = boxInformationSAP.ConfirmationNumber,
                    ContainerId = boxInformationSAP.ContainerId,
                    DivertLane = boxInformationSAP.DivertLane,
                    SAPSystem = boxInformationSAP.SAPSystem,
                    Qty = boxInformationSAP.Qty,
                    Status = boxInformationSAP.Status
                });

        
            var result = await _routingInfoController.GetById(searchBoxId, trackingId);

      
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        }



        [Test]
        public async Task MultiBox_DivertBox_Testing()
        {
            var boxInformationSAP = new WCSRoutingV10
            {
                Id = 1,
                BoxId = "C1006881659",
                BoxType = "IBOX_D2",
                CarrierCode = "UPSGC",
                ContainerType = "T",
                CurrentTs = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                LogisticAgent = "Z023",
                ContainerId = "1000001179",
                ConfirmationNumber = "C1006881670",
                DivertLane = 2,
                Qty = 2,
                SAPSystem = "AFS1",
                Status = "NA"
            };

            string searchBoxId = "C1006881659";
            int trackingId = 1;

            _routingServiceMock.Setup(service => service.GetOrdersBoxIdSAPInformation(searchBoxId, trackingId))
                .ReturnsAsync(new RoutingV10Dto
                {
                    Id = 1,
                    BoxId = boxInformationSAP.BoxId,
                    BoxType = boxInformationSAP.BoxType,
                    CarrierCode = boxInformationSAP.CarrierCode,
                    ContainerType = boxInformationSAP.ContainerType,
                    LogisticAgent = boxInformationSAP.LogisticAgent,
                    CurrentTs = boxInformationSAP.CurrentTs,
                    ConfirmationNumber = boxInformationSAP.ConfirmationNumber,
                    ContainerId = boxInformationSAP.ContainerId,
                    DivertLane = boxInformationSAP.DivertLane,
                    SAPSystem = boxInformationSAP.SAPSystem,
                    Qty = boxInformationSAP.Qty,
                    Status = boxInformationSAP.Status
                });

            var validModel = new DivertBoxReqDto
            {
                BoxId = "C1006881659",
                TrackingId = 1
            };

         
            var result = await _controller.Post(validModel);

            Assert.NotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task GetById_BoxIdNotFound_ReturnsNotFound()
        {
            // Arrange
            string nonExistentBoxId = "C43343";
            int trackingId = 1;

            _routingServiceMock.Setup(service => service.GetOrdersBoxIdSAPInformation(nonExistentBoxId, trackingId))
                .ReturnsAsync((RoutingV10Dto)null);

            // Act
            var result = await _routingInfoController.GetById(nonExistentBoxId, trackingId);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = (NotFoundObjectResult)result;


            Assert.IsNotNull(notFoundResult.Value);
            Assert.AreEqual(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }
    }
}
