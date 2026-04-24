using Luxottica.ApplicationServices.CommissionerPackingLimits;
using Luxottica.ApplicationServices.Commissioners;
using Luxottica.ApplicationServices.Shared.Dto.CommisionerPackingLimits;
using Luxottica.ApplicationServices.Shared.Dto.DivertLines;
using Luxottica.Controllers.CommissionerPackingLimits;
using Luxottica.Controllers.Commissioners;
using Luxottica.Core.Entities.EXT;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.UnitTest.CommisionerPackingLimits
{
    [TestFixture]
    public class CommisionerPackingLimitTest
    {
        private CommissionerPackingLimitsController _controller;
        private Mock<ICommissionerPackingLimitAppService> _commissionersPackingServiceMock;
        private Mock<ILogger<CommissionerPackingLimitsController>> _loggerMock;
        [SetUp]
        public void Setup()
        {
            _commissionersPackingServiceMock = new Mock<ICommissionerPackingLimitAppService>();
            _loggerMock = new Mock<ILogger<CommissionerPackingLimitsController>>();
            _controller = new CommissionerPackingLimitsController(
                _commissionersPackingServiceMock.Object, _loggerMock.Object
            );
        }

        [Test]
        public async Task GetAll_CommissionerLimits()
        {
            // Arrange
            var commisionerPackingDtos = new List<Commissioner_Packing_Limits>
            {
                new Commissioner_Packing_Limits { Id = 1, PutStationNr = 1, Limit = 10 },
                new Commissioner_Packing_Limits { Id = 2, PutStationNr = 2, Limit = 15 },
                new Commissioner_Packing_Limits { Id = 1, PutStationNr = 1, Limit = 20 },
                new Commissioner_Packing_Limits { Id = 2, PutStationNr = 2, Limit = 25 },
            };

            _commissionersPackingServiceMock.Setup(service => service.GetLimitsPacking())
                .ReturnsAsync(commisionerPackingDtos);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Test]
        public async Task UpdateLimits_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var request = new CommisionerPackingLimitsRequest
            {
                SetLimitSuresort_1 = 0, // Establecer un límite no válido
                SetLimitSuresort_2 = 20,
                SetLimitPutWall_1 = 30,
                SetLimitPutWall_2 = 40
            };

            // Act
            var result = await _controller.UpdateLimits(request);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task UpdateLimits_ValidRequest_ReturnsOk()
        {
            // Arrange
            var request = new CommisionerPackingLimitsRequest
            {
                SetLimitSuresort_1 = 10,
                SetLimitSuresort_2 = 20,
                SetLimitPutWall_1 = 30,
                SetLimitPutWall_2 = 40
            };

            // Act
            var result = await _controller.UpdateLimits(request);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }
    }
}
