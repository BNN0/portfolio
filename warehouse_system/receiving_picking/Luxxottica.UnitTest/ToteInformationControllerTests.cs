using Luxottica.ApplicationServices.ToteInformations;
using Luxottica.Controllers.Totes;
using Luxottica.Core.Entities.Cameras;
using Luxottica.Core.Entities.ToteInformations;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxxottica.UnitTest
{
    public class ToteInformationControllerTests
    {
        private Mock<IToteInformationAppService> _toteServiceMock;
        private ToteInformationController _controller;

        [SetUp]
        public void Setup()
        {
            _toteServiceMock = new Mock<IToteInformationAppService>();
            _controller = new ToteInformationController(_toteServiceMock.Object);
        }

        [Test]
        public async Task TestGetAll()
        {
            // Arrange
            _toteServiceMock.Setup(service => service.GetTotesAsync())
                .ReturnsAsync(new List<ToteInformation>());

            // Act
            var result = await _controller.GetAll();

            // Assert
            var totes = result as List<ToteInformation>;
            Assert.NotNull(totes);
            Assert.IsEmpty(totes);
        }

        [Test]
        public async Task TestGetById()
        {
            // Arrange
            int testId = 1;
            _toteServiceMock.Setup(service => service.GetToteInformationByIdAsync(testId))
                .ReturnsAsync(new ToteInformation 
                { 
                    Id = testId,
                    ToteLPN = "string", 
                    ReqTimestamp = "string",
                    VirtualTote = "string",
                    ZoneDivertId= 0,
                    DivertStatus = true,
                    LineCount = 0
                });

            // Act
            var result = await _controller.GetById(testId);

            // Assert
            var tote = result as ToteInformation;
            Assert.NotNull(tote);
            Assert.AreEqual(testId, tote.Id);
        }
        [Test]
        public async Task TestEdit()
        {
            // Arrange
            int testId = 1;
            var editedTote = new ToteInformation 
            { 
                Id = testId,
                ToteLPN = "Editedstring",
                ReqTimestamp = "Editedstring",
                VirtualTote = "Editedstring",
                ZoneDivertId = 1,
                DivertStatus = true,
                LineCount = 1
            };
            _toteServiceMock.Setup(service => service.EditToteInformationAsync(editedTote));

            // Act
            await _controller.Put(testId, editedTote);

            // Assert
            _toteServiceMock.Verify(service => service.EditToteInformationAsync(editedTote), Times.Once);
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
            var newTote = new ToteInformation {
                ToteLPN = "string",
                ReqTimestamp = "string",
                VirtualTote = "string",
                ZoneDivertId = 0,
                DivertStatus = true,
                LineCount = 0
            };
            _toteServiceMock.Setup(service => service.AddToteInformationAsync(newTote));

            // Act
            var result = await _controller.Post(newTote);

            // Assert
            Assert.NotNull(result);
            _toteServiceMock.Verify(service => service.AddToteInformationAsync(newTote), Times.Once);
        }
    }

}


