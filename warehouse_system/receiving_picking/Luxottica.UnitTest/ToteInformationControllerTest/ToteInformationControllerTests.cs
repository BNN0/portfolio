using Luxottica.ApplicationServices.DivertLines;
using Luxottica.ApplicationServices.JackpotLines;
using Luxottica.ApplicationServices.PhysicalMaps;
using Luxottica.ApplicationServices.Scanlogs;
using Luxottica.ApplicationServices.ToteInformations;
using Luxottica.Controllers.Totes;
using Luxottica.Controllers.Totes.Luxottica.Controllers.PhysicalMaps;
using Luxottica.Core.Entities.Cameras;
using Luxottica.Core.Entities.ToteInformations;
using Luxottica.Models.NewTote;
using Luxottica.Models.Tote;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.UnitTest.ToteInformationControllerTest
{
    public class ToteInformationControllerTests
    {
        private Mock<HttpClient> _http; 
        private Mock<IToteInformationAppService> _toteServiceMock;
        private Mock<IJackpotLineAppService> _jackpotLineAppServiceMock;
        private Mock<IDivertLineService> _divertLineServiceMock;
        private Mock<IMapPhysicalAppService> _mapPhysicalAppServiceMock;
        private Mock<ILogger<ToteInformationController>> _loggerMock;
        private ToteInformationController _controller;
        private Mock<IScanlogsAppService> _scanlogsAppService;

        [SetUp]
        public void Setup()
        {
            _toteServiceMock = new Mock<IToteInformationAppService>();
            _http = new Mock<HttpClient>();
            _jackpotLineAppServiceMock = new Mock<IJackpotLineAppService>();
            _divertLineServiceMock = new Mock<IDivertLineService>();
            _mapPhysicalAppServiceMock = new Mock<IMapPhysicalAppService>();
            _scanlogsAppService = new Mock <IScanlogsAppService>();
            _loggerMock = new Mock<ILogger<ToteInformationController>>();
           _controller = new ToteInformationController(_toteServiceMock.Object,_http.Object, _jackpotLineAppServiceMock.Object, _divertLineServiceMock.Object, _mapPhysicalAppServiceMock.Object,_loggerMock.Object, _scanlogsAppService.Object);
        }

        [Test]
        public async Task TestGetAll()
        {
            // Arrange
            _toteServiceMock.Setup(service => service.GetTotesAsync())
                .ReturnsAsync(new List<ToteInformationE>());

            // Act
            var result = await _controller.GetAll();

            // Assert
            var totes = result as List<ToteInformationE>;
            Assert.NotNull(totes);
            Assert.IsEmpty(totes);
        }

        [Test]
        public async Task TestGetById()
        {
            // Arrange
            int testId = 1;
            _toteServiceMock.Setup(service => service.GetToteInformationByIdAsync(testId))
                .ReturnsAsync(new ToteInformationE
                {
                    Id = testId,
                    ToteLPN = "string",
                    Timestamp = "string",
                    VirtualTote = "string",
                    ZoneDivertId = 0,
                    DivertStatus = "DS",
                    LineCount = 0
                });

            // Act
            var result = await _controller.GetById(testId);

            // Assert
            var tote = result as ToteInformationE;
            Assert.NotNull(tote);
            Assert.AreEqual(testId, tote.Id);
        }
        [Test]
        public async Task TestEdit()
        {
            // Arrange
            int testId = 1;
            var editedTote = new ToteInformationE
            {
                Id = testId,
                ToteLPN = "1string",
                VirtualTote = "1qwert",
                ZoneDivertId = 0,
                DivertStatus = "DS",
                LineCount = 0
            };
            var editedTote1 = new ToteModel
            {
                toteLpn = "1string",
                VirtualTote = "1qwert",
                ZoneDivertId = 0,
                DivertStatus = "DS",
                LineCount = 0
            };
            _toteServiceMock.Setup(service => service.EditToteInformationAsync(editedTote));

            // Act
            await _controller.Put(testId, editedTote1);

            // Assert
            _toteServiceMock.Verify(service => service.EditToteInformationAsync(editedTote), Times.Never);
        }

        [Test]
        public async Task TestDelete()
        {
            // Arrange
            int testId = 1;
            _toteServiceMock.Setup(service => service.DeleteToteInformationAsync(testId));

            // Act
            await _controller.Delete(testId);

            // Assert
            _toteServiceMock.Verify(service => service.DeleteToteInformationAsync(testId), Times.Once);
        }
        [Test]
        public async Task TestAdd()
        {
            // Arrange
            var newTote = new ToteInformationE
            {
                ToteLPN = "HC99826001",
                VirtualTote = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567",
                ZoneDivertId = 0,
                DivertStatus = "st",
                LineCount = 0
            };
            var newTote1 = new ToteModel
            {
                toteLpn = "HC99826001",
                VirtualTote = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567",
                ZoneDivertId = 0,
                DivertStatus = "st",
                LineCount = 0
            };
            _toteServiceMock.Setup(service => service.AddToteInformationAsync(newTote));

            // Act
            var result = await _controller.Post(newTote1);

            // Assert
            Assert.NotNull(result);
        }

    }

}


