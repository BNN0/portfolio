using LuxotticaSorting.ApplicationServices.LogisticAgents;
using LuxotticaSorting.ApplicationServices.Shared.Dto.LogisticAgents;
using LuxotticaSorting.Controllers.LogisticAgents;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LuxxoticaSorting.Unitest.LogisticAgentsTest
{
    public class LogisticAgentTest
    {
        private Mock<ILogger<LogisticAgentController>> _loggerLogisticAgentMock;
        private Mock<ILogisticAgentAppService> _logisticAgentServiceMock;
        private LogisticAgentController _LogisticAgentcontroller;

        [SetUp]
        public void Setup()
        {
            _loggerLogisticAgentMock = new Mock<ILogger<LogisticAgentController>>();
            _logisticAgentServiceMock = new Mock<ILogisticAgentAppService>();
            _LogisticAgentcontroller = new LogisticAgentController(_logisticAgentServiceMock.Object, _loggerLogisticAgentMock.Object);
        }
        [Test]
        public async Task TestAddLogisticAgent()
        {
            // Arrange
            var newlogisticAgent = new LogisticAgentAddDto { LogisticAgents = "AGENT0001" };

            _logisticAgentServiceMock.Setup(service => service.AddLogisticAgentAsync(newlogisticAgent));

            // Act
            var result = await _LogisticAgentcontroller.Post(newlogisticAgent);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Test]
        public async Task GetAllLogisticAgent()
        {
            // Arrange
            var logisticAgentDto = new List<LogisticAgentDto>
                {
                    new LogisticAgentDto { Id = 1, LogisticAgents = "AGENT0001"},
                    new LogisticAgentDto { Id = 2, LogisticAgents = "AGENT0002" },
                };

            _logisticAgentServiceMock.Setup(service => service.GetLogisticAgentsAsync())
                .ReturnsAsync(logisticAgentDto);

            // Act
            var result = await _LogisticAgentcontroller.GetAll();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

            var logisticAgent = okResult.Value as List<LogisticAgentDto>;
            Assert.NotNull(logisticAgent);
            Assert.AreEqual(2, logisticAgent.Count);
        }

        [Test]
        public async Task GetByIdLogisticAgent()
        {
            // Arrange
            int testId = 1;
            _logisticAgentServiceMock.Setup(service => service.GetLogisticAgentByIdAsync(testId))
                .ReturnsAsync(new LogisticAgentDto { Id = testId, LogisticAgents = "AGENT0001" });

            // Act
            var result = await _LogisticAgentcontroller.GetById(testId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

            var logisticAgent = okResult.Value as LogisticAgentDto;
            Assert.NotNull(logisticAgent);
            Assert.AreEqual(testId, logisticAgent.Id);
        }
        [Test]
        public async Task TestEditLogisticAgent()
        {
            // Arrange
            int testId = 1;
            var editLogisticAgent = new LogisticAgentAddDto { LogisticAgents = "AGENT000N2" };
            _logisticAgentServiceMock.Setup(service => service.EditLogisticAgentAsync(testId, editLogisticAgent));

            // Act
            var result = await _LogisticAgentcontroller.Put(testId, editLogisticAgent);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Test]
        public async Task TestDeleteLogisticAgent()
        {
            // Arrange
            int testId = 1;

            _logisticAgentServiceMock.Setup(service => service.DeleteLogisticAgentAsync(testId));

            // Act
            var result = await _LogisticAgentcontroller.Delete(testId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        }
    }
}
