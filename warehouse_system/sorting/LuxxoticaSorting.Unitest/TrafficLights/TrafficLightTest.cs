using LuxotticaSorting.ApplicationServices.Shared.DTO.TrafficLights;
using LuxotticaSorting.ApplicationServices.TrafficLights;
using LuxotticaSorting.Controllers.TrafficLights;
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

namespace LuxxoticaSorting.Unitest.TrafficLights
{
    public class TrafficLightTest
    {
        private Mock<ITrafficLightAppService> _trafficServiceMock;
        private TrafficLightController _trafficController;
        private Mock<ILogger<TrafficLightController>> _loggerMock;

        [SetUp]
        public void Setup()
        {
            _trafficServiceMock = new Mock<ITrafficLightAppService> ();
            _loggerMock = new Mock<ILogger<TrafficLightController>> ();
            _trafficController = new TrafficLightController(_trafficServiceMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task GetInfoTrafficLightLine2()
        {
            //Arrange
            var trafficInfo = new TrafficLightDataDto
            {
                LigthGreen = true,
                LigthRed = false
            };
            _trafficServiceMock.Setup(service => service.GetStatusLightLine2()).ReturnsAsync(trafficInfo);

            //Act
            var result = await _trafficController.GetStatusTrafficLightLine2();

            //Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

            var trafficData = okResult.Value as TrafficLightDataDto;
            Assert.IsNotNull(trafficData);
            Assert.IsAssignableFrom<TrafficLightDataDto>(trafficData);
        }
        [Test]
        public async Task GetInfoTrafficLightLine4()
        {
            //Arrange
            var trafficInfo = new TrafficLightDataDto
            {
                LigthGreen = false,
                LigthRed = true
            };
            _trafficServiceMock.Setup(service => service.GetStatusLightLine4()).ReturnsAsync(trafficInfo);

            //Act
            var result = await _trafficController.GetStatusTrafficLightLine4();

            //Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

            var trafficData = okResult.Value as TrafficLightDataDto;
            Assert.IsNotNull(trafficData);
            Assert.IsAssignableFrom<TrafficLightDataDto>(trafficData);
        }
    }
}
