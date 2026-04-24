using Luxottica.ApplicationServices.Acknowledgments;
using Luxottica.ApplicationServices.Cameras;
using Luxottica.ApplicationServices.Shared.Dto.Acknowledgment;
using Luxottica.ApplicationServices.Shared.Dto.Camera;
using Luxottica.Controllers.Acknowledgments;
using Luxottica.Controllers.Cameras;
using Luxottica.Core.Entities.Acknowledgments;
using Luxottica.Core.Entities.Cameras;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.UnitTest.AcknowledgmentsTest
{
    [TestFixture]
    public class AcknowledgmentsTest
    {
        private Mock<IAcknowledgmentAppService> _acknowledgmentServiceMock;
        private AcknowledgmentController _controller;

        [SetUp]
        public void Setup()
        {
            _acknowledgmentServiceMock = new Mock<IAcknowledgmentAppService>();
            _controller = new AcknowledgmentController(_acknowledgmentServiceMock.Object);
        }
        [Test]
        public async Task GetAll_ReturnsOkResultWithListOfAcknowledgments()
        {
            // Arrange
            var acknowledgments = new List<Acknowledgment>
            {
                new Acknowledgment {  },
                new Acknowledgment {  }
            };

            _acknowledgmentServiceMock.Setup(service => service.GetAcknowledgmentsAsync()).ReturnsAsync(acknowledgments);

            // Act
            var result = await _controller.GetAll() as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.IsInstanceOf<List<Acknowledgment>>(result.Value);
            Assert.AreEqual(acknowledgments.Count, ((List<Acknowledgment>)result.Value).Count);
        }

        [Test]
        public async Task GetById_InvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            int invalidId = -1; // An invalid ID that doesn't exist

            _acknowledgmentServiceMock.Setup(service => service.GetAcknowledgmentByIdAsync(invalidId)).ReturnsAsync((Acknowledgment)null);

            // Act
            var result = await _controller.GetById(invalidId) as NotFoundObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        [Test]
        public async Task Post_ValidAcknowledgment_ReturnsOkResult()
        {
            // Arrange
            var validAcknowledgment = new AcknowledgmentAddDTO
            {
                Status = "IN",
                ToteLpn = "1234567890",
                WaveNr = "1234567890"
            };

            _acknowledgmentServiceMock.Setup(service => service.AddAcknowledgmentAsync(validAcknowledgment)).ReturnsAsync(1); // Assuming it returns the ID of the added acknowledgment.

            // Act
            var result = await _controller.Post(validAcknowledgment) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }


        [Test]
        public async Task Post_InvalidAcknowledgment_ReturnsBadRequestResult()
        {
            // Arrange
            var invalidAcknowledgment = new AcknowledgmentAddDTO
            {
                ToteLpn = "InvalidLpnThatIsTooLong", // Longer than 10 characters
                WaveNr = "ValidWaveNr",
                Status = "InvalidStatusThatIsTooLong" // Longer than 2 characters
            };

            // Act
            var result = await _controller.Post(invalidAcknowledgment) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task Put_ValidAcknowledgment_ReturnsOkResult()
        {
            // Arrange
            var validAcknowledgment = new AcknowledgmentAddDTO
            {
                Status = "IN",
                ToteLpn = "1234567890",
                WaveNr = "1234567890"
            };

            _acknowledgmentServiceMock.Setup(service => service.EditAcknowledgmentAsync(1, validAcknowledgment));

            // Act
            var result = await _controller.Put(1, validAcknowledgment) as OkResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task Put_InvalidAcknowledgment_ReturnsBadRequestResult()
        {
            // Arrange
            var invalidAcknowledgment = new AcknowledgmentAddDTO
            {

                Status = "InvalidStatus", // Invalid status

            };

            // Act
            var result = await _controller.Put(1, invalidAcknowledgment) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }
        [Test]
        public async Task Delete_NonExistingAcknowledgment_ReturnsNotFoundResult()
        {
            // Arrange
            var nonExistingAcknowledgmentId = 999; // Assuming this ID does not exist in the data.

            _acknowledgmentServiceMock.Setup(service => service.DeleteAcknowledgmentAsync(nonExistingAcknowledgmentId))
                .ThrowsAsync(new NotFoundException("Acknowledgment not found."));

            // Act
            var result = await _controller.Delete(nonExistingAcknowledgmentId) as NotFoundObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        [Test]
        public async Task GetAllByWave_ExistingWave_ReturnsOkResult()
        {
            // Arrange
            var existingWave = "ValidWave"; // Assuming an existing wave.

            var acknowledgments = new List<Acknowledgment>
            {
                new Acknowledgment { Id = 1, WaveNr = "ValidWave" }, // Acknowledgment with matching wave
                new Acknowledgment { Id = 2, WaveNr = "AnotherWave" }, // Acknowledgment with different wave
            };

            _acknowledgmentServiceMock.Setup(service => service.GetAcknowledgmentsAsync()).ReturnsAsync(acknowledgments);

            // Act
            var result = await _controller.GetAllByWave(existingWave) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }
        [Test]
        public async Task GetAllByWave_NonExistingWave_ReturnsNotFoundResult()
        {
            // Arrange
            var nonExistingWave = "NonExistentWave"; // Assuming this wave does not exist in the data.

            var acknowledgments = new List<Acknowledgment>
            {
                new Acknowledgment { Id = 1, WaveNr = "ValidWave" }, // Acknowledgment with different wave
                new Acknowledgment { Id = 2, WaveNr = "AnotherWave" }, // Acknowledgment with another different wave
            };

            _acknowledgmentServiceMock.Setup(service => service.GetAcknowledgmentsAsync()).ReturnsAsync(acknowledgments);

            // Act
            var result = await _controller.GetAllByWave(nonExistingWave) as NotFoundObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        [Test]
        public async Task GetAllByStatus_ExistingStatus_ReturnsOkResult()
        {
            // Arrange
            var existingStatus = "IN"; // Assuming an existing status.

            var acknowledgments = new List<Acknowledgment>
            {
                new Acknowledgment { Id = 1, Status = "IN" }, // Acknowledgment with matching status
                new Acknowledgment { Id = 2, Status = "NA" }, // Acknowledgment with matching status
                new Acknowledgment { Id = 3, Status = "OUT" }, // Acknowledgment with different status
            };

            _acknowledgmentServiceMock.Setup(service => service.GetAcknowledgmentsAsync()).ReturnsAsync(acknowledgments);

            // Act
            var result = await _controller.GetAllByStatus(existingStatus) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task GetAllByStatus_NonExistingStatus_ReturnsNotFoundResult()
        {
            // Arrange
            var nonExistingStatus = "NonExistentStatus"; // Assuming this status does not exist in the data.

            var acknowledgments = new List<Acknowledgment>
            {
                new Acknowledgment { Id = 1, Status = "IN" }, // Acknowledgment with different status
                new Acknowledgment { Id = 2, Status = "NA" }, // Acknowledgment with another different status
            };

            _acknowledgmentServiceMock.Setup(service => service.GetAcknowledgmentsAsync()).ReturnsAsync(acknowledgments);

            // Act
            var result = await _controller.GetAllByStatus(nonExistingStatus) as NotFoundObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
            Assert.IsInstanceOf<NotFoundObjectResult>(result);

        }

    }

}


