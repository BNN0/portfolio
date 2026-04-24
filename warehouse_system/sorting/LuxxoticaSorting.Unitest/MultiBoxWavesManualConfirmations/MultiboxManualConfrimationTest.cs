using LuxotticaSorting.ApplicationServices.MultiBoxWaves;
using LuxotticaSorting.ApplicationServices.Shared.DTO.MultiBoxWaves;
using LuxotticaSorting.Controllers.MultiBoxWaves;
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

namespace LuxxoticaSorting.Unitest.MultiBoxWavesManualConfirmations
{
    public class MultiboxManualConfrimationTest
    {
        private Mock<IMultiBoxWaveAppService> _multiboxServiceMock;
        private MultiBoxWaveController _multiboxControllerMock;
        private Mock<ILogger<MultiBoxWaveController>> _loggerMock;

        [SetUp]
        public void Setup()
        {
            _multiboxServiceMock = new Mock<IMultiBoxWaveAppService>();
            _loggerMock = new Mock<ILogger<MultiBoxWaveController>>();
            _multiboxControllerMock = new MultiBoxWaveController(_multiboxServiceMock.Object,_loggerMock.Object);
        }

        [Test]
        public async Task ManualConfrimation()
        {
            //Arrange
            var confirmationDataList = new List<MultiBoxWaveConfirmationDto>
            {
                new MultiBoxWaveConfirmationDto{BoxId = "C000011",ConfirmationNumber = "CN90990909"},
                new MultiBoxWaveConfirmationDto{BoxId = "C000021",ConfirmationNumber = "CN90990909"},
                new MultiBoxWaveConfirmationDto{BoxId = "C000022",ConfirmationNumber = "CN90990909"},
                new MultiBoxWaveConfirmationDto{BoxId = "C0000012",ConfirmationNumber = "CN90990988"},
                new MultiBoxWaveConfirmationDto{BoxId = "C0000013",ConfirmationNumber = "CN90990988"},
                new MultiBoxWaveConfirmationDto{BoxId = "C0000014",ConfirmationNumber = "CN90990988"},
                new MultiBoxWaveConfirmationDto{BoxId = "C0000015",ConfirmationNumber = "CN90990988"},
            };


            foreach (var manualConfirmation in confirmationDataList)
            {
                
                _multiboxServiceMock.Setup(service => service.ManualConfirmationMultiBoxWavesForTrucks(manualConfirmation))
                    .ReturnsAsync(true);

                // Act
                var result = await _multiboxControllerMock.ManualConfirmationMultiBoxWaves(manualConfirmation);

                // Assert
                var okResult = result as OkObjectResult;
                Assert.IsNotNull(okResult);
                Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
                var responseMessage = okResult.Value;
                Assert.NotNull(responseMessage);
            }
        }

        [Test]
        public async Task ManualConfrimationInvalid()
        {
            //Arrange
            var confirmationDataList = new List<MultiBoxWaveConfirmationDto>
            {
                new MultiBoxWaveConfirmationDto{BoxId = "",ConfirmationNumber = "CN90990909"},
                new MultiBoxWaveConfirmationDto{BoxId = "C000021",ConfirmationNumber = ""},
                new MultiBoxWaveConfirmationDto{BoxId = "",ConfirmationNumber = ""}
            };


            foreach (var manualConfirmation in confirmationDataList)
            {

                _multiboxServiceMock.Setup(service => service.ManualConfirmationMultiBoxWavesForTrucks(manualConfirmation))
                    .ReturnsAsync(false);

                // Act
                var result = await _multiboxControllerMock.ManualConfirmationMultiBoxWaves(manualConfirmation);

                // Assert
                var okResult = result as BadRequestObjectResult;
                Assert.IsNotNull(okResult);
                Assert.AreEqual(StatusCodes.Status400BadRequest, okResult.StatusCode);
                var responseMessage = okResult.Value;
                Assert.NotNull(responseMessage);
            }
        }
    }
}
