using Luxottica.ApplicationServices.Users;
using Luxottica.Controllers.Users;
using Luxottica.Controllers.Versions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.UnitTest.VersionTest
{
    public class VersionTest
    {
        private VersionController _controller;
        private Mock<ILogger<VersionController>> _loggerMock;

        [SetUp]
        public void Setup()
        {
            var loggerMock = new Mock<ILogger<VersionController>>();
            _controller = new VersionController(loggerMock.Object);
        }

        [Test]
        public async Task Get_ReturnsOkResult_WithValidVersion()
        {
            // Arrange


            // Act
            var result = await _controller.Get() as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);

        }



        [Test]
        public async Task GetAll_ReturnsOkResult_WithValidVersions()
        {
            // Arrange


            // Act
            var result = await _controller.GetAll() as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);


        }

    }
}


