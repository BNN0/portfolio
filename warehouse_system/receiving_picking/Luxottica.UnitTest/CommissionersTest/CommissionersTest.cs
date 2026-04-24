using Luxottica.ApplicationServices.Commissioners;
using Luxottica.ApplicationServices.Shared.Dto.Camera;
using Luxottica.Controllers.Commissioners;
using Luxottica.Core.Entities.Cameras;
using Luxottica.Core.Entities.Commissioners;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.UnitTest.CommissionersTest
{
    [TestFixture]
    public class CommissionerControllerTests
    {
        private CommissionerController _controller;
        private Mock<ICommissionersAppService> _commissionersServiceMock;
        private Mock<ILogger<CommissionerController>> _loggerMock;
        [SetUp]
        public void Setup()
        {
            _commissionersServiceMock = new Mock<ICommissionersAppService>();
            _loggerMock = new Mock<ILogger<CommissionerController>>();
            _controller = new CommissionerController(
                _commissionersServiceMock.Object, _loggerMock.Object
            );
        }
        [Test]
        public async Task GetStatusOfFirstCommissioner_Returns_CorrectResult()
        {
            // Arrange
            var commissioner = new Commissioner { Id = 1, Status = true };
            _commissionersServiceMock.Setup(service => service.GetComissionnersAsync())
                .ReturnsAsync(new List<Commissioner> { commissioner });

            // Act
            var result = await _controller.GetStatusOfFirstCommissioner();

            // Assert
            Assert.NotNull(result);
        }



        [Test]
        public async Task UpdateStatus_ReturnsOk()
        {
            // Arrange
            var commissioner = new Commissioner { Id = 1, Status = true };
            _commissionersServiceMock.Setup(service => service.GetFirstCommissionerAsync())
                .ReturnsAsync(commissioner);

            // Act
            var result = await _controller.ChangeCommissionerStatus(false);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task UpdateStatus_Exception_ReturnsInternalServerError()
        {
            // Arrange
            _commissionersServiceMock.Setup(service => service.GetFirstCommissionerAsync())
                .Throws(new Exception("Test exception"));

            // Act
            var result = await _controller.ChangeCommissionerStatus(true);

            // Assert
            Assert.IsInstanceOf<ObjectResult>(result);
            Assert.AreEqual(500, (result as ObjectResult).StatusCode);
        }
    }

}
