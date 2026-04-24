using LuxotticaSorting.ApplicationServices.RecirculationLimits;
using LuxotticaSorting.ApplicationServices.Shared.DTO.RecirculationLimits;
using LuxotticaSorting.Controllers.RecirculationLimits;
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

namespace LuxxoticaSorting.Unitest.RecirculationLimits
{
    public class RecirculationLimitTest
    {
        private Mock<IRecirculationLimitAppService> _recirculationServiceMock;
        private RecirculationLimitController _recirculationController;
        private Mock<ILogger<RecirculationLimitController>> _loggerMock;

        [SetUp]
        public void Setup()
        {
            _recirculationServiceMock = new Mock<IRecirculationLimitAppService>();
            _loggerMock = new Mock<ILogger<RecirculationLimitController>>();
            _recirculationController = new RecirculationLimitController( _recirculationServiceMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task TestGetRecirculationValue()
        {
            //Arrange
            var expectedRecirculation = new List<RecirculationLimitDto>
            {
                new RecirculationLimitDto {Id = 1, CountLimit = 30}
            };

            _recirculationServiceMock.Setup(service => service.GetRecirculationLimitValue())
                .ReturnsAsync(expectedRecirculation);

            //Act
            var result = await _recirculationController.GetAll();

            //Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

            var recirculationvalue = okResult.Value as List<RecirculationLimitDto>;
            Assert.NotNull(recirculationvalue);
            Assert.AreEqual(expectedRecirculation.Count, recirculationvalue.Count);
        }

        [Test]
        public async Task TestEditValueRecirculationlimit()
        {
            //Arrange 
            var editValue = new RecirculationLimitAddDto { CountLimit = 20 };
            _recirculationServiceMock.Setup(service => service.EditRecirculationLimitAddDto(editValue));

            //Act
            await _recirculationController.ChangeValueRecirculationLimit(editValue);

            _recirculationServiceMock.Verify(service => service.EditRecirculationLimitAddDto(editValue), Times.Once);
        }
    }
}
