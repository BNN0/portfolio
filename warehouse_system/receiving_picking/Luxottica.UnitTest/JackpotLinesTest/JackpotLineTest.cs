using Luxottica.ApplicationServices.JackpotLines;
using Luxottica.ApplicationServices.Shared.Dto.JackpotLines;
using Luxottica.Controllers.DivertLines;
using Luxottica.Controllers.JackpotLine;
using Luxottica.Controllers.LimitSettings;
using Luxottica.Controllers.PhysicalMaps;
using Microsoft.Extensions.Logging;
using Moq;

namespace Luxottica.UnitTest.JackpotLinesTest
{
    public class JackpotLineTest
    {
        private Mock<IJackpotLineAppService> _jakpotMock;
        private JackpotLineController _controller;

        [SetUp]
        public void Setup()
        {
            _jakpotMock = new Mock<IJackpotLineAppService>();
            var loggerMock = new Mock<ILogger<JackpotLineController>>();
            _controller = new JackpotLineController(_jakpotMock.Object, loggerMock.Object);
        }

        [Test]
        public async Task TestAdd()
        {
            // Arrange
            var jackopot = new JackpotLineAddDto { DivertLineId = 1, JackpotLineValue = true };

            _jakpotMock.Setup(service => service.AddJackpotLinesAsync(jackopot));

            // Act
            await _controller.Post(jackopot);

            // Assert
            _jakpotMock.Verify(service => service.AddJackpotLinesAsync(jackopot), Times.Once);
        }

        [Test]
        public async Task TestGetAll()
        {
            // Arrange
            _jakpotMock.Setup(service => service.GetJackpotLinesAsync())
                .ReturnsAsync(new List<JackpotLineDto>());

            // Act
            var result = await _controller.GetAll();

            // Assert
            var jackpot = result as List<JackpotLineDto>;
            Assert.NotNull(jackpot);
            Assert.IsEmpty(jackpot);
        }

        [Test]
        public async Task TestGetById()
        {
            // Arrange
            int testId = 1;
            _jakpotMock.Setup(service => service.GetJackpotLineByIdAsync(testId))
                .ReturnsAsync(new JackpotLineDto { Id = testId });

            // Act
            var result = await _controller.GetById(testId);

            // Assert
            var jackpot = result as JackpotLineDto;
            Assert.NotNull(jackpot);
            Assert.AreEqual(testId, jackpot.Id);
        }

        [Test]
        public async Task TestEdit()
        {
            // Arrange
            int testId = 1;
            var editJack = new JackpotLineAddDto { DivertLineId = 1, JackpotLineValue = false };
            _jakpotMock.Setup(service => service.EditJackpotLinesAsync(testId, editJack));

            // Act
            await _controller.Put(testId, editJack);

            // Assert
            _jakpotMock.Verify(service => service.EditJackpotLinesAsync(testId, editJack), Times.Once);
        }

        [Test]
        public async Task TestDelete()
        {
            // Arrange
            int testId = 1;
            _jakpotMock.Setup(service => service.DeleteJackpotLinesAsync(testId));

            // Act
            await _controller.Delete(testId);

            // Assert
            _jakpotMock.Verify(service => service.DeleteJackpotLinesAsync(testId), Times.Once);
        }
    }
}
