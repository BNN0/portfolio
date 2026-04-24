using LuxotticaSorting.ApplicationServices.PrintLabel;
using LuxotticaSorting.ApplicationServices.Shared.DTO.Containers;
using LuxotticaSorting.ApplicationServices.Shared.DTO.PrintLabel;
using LuxotticaSorting.ApplicationServices.Shared.DTO.Zebra;
using LuxotticaSorting.Controllers.Containers;
using LuxotticaSorting.Controllers.PrintLabel;
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

namespace LuxxoticaSorting.Unitest.PrintLabel
{
    [TestFixture]
    public class PrintLabelTest
    {
        private PrintLabelController _printLabelController;
        private Mock<IPrintLabelAppService> _printLabelAppService;
        private Mock<ILogger<PrintLabelController>> _logger;

        [SetUp]
        public void Setup()
        {
            _printLabelAppService = new Mock<IPrintLabelAppService>();
            _logger = new Mock<ILogger<PrintLabelController>>();

            _printLabelController = new PrintLabelController(_printLabelAppService.Object, _logger.Object);
        }

        [Test]
        public void Post_WhenValidData_ReturnsOk()
        {
            // Arrange
            var printLabelDTO = new PrintLabelDTO
            {
                //LogisticAgentId= 1,
                ContainerId= 1,
                DivertLaneId= 1,
                //CarrierCodeId= 1,
            };

            _printLabelAppService.Setup(service => service.PrintLabelService(printLabelDTO)).ReturnsAsync(true);

            // Act
            var result = _printLabelController.Post(printLabelDTO).Result as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
        }

        [Test]
        public void PostReprint_WhenValidData_ReturnsOk()
        {
            // Arrange
            var printLabelDTO = new PrintLabelReprint
            {
                ContainerId =1,

                ZebraConfigurationId = 1
            };

            _printLabelAppService.Setup(service => service.PrintLabelReprint(printLabelDTO)).ReturnsAsync(true);

            // Act
            var result = _printLabelController.RePrintLabel(printLabelDTO).Result as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
        }

        [Test]
        public void PostManual_WhenValidData_ReturnsOk()
        {
            // Arrange
            var printLabelDTO = new PrintManualDTO
            {
                ContainerId = 1,
            };

            _printLabelAppService.Setup(service => service.PrintLabelManualService(printLabelDTO)).ReturnsAsync(true);

            // Act
            var result = _printLabelController.PrintLabelManual(printLabelDTO).Result as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
        }

        [Test]
        public void Post_WhenInvalidData_ReturnsBadRequest()
        {
            // Arrange
            PrintLabelDTO printLabelDTO = null; 

            // Act
            var result = _printLabelController.Post(printLabelDTO).Result as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Invalid JSON Model!.", result.Value);
        }

        [Test]
        public void Post_WhenServiceFails_ReturnsInternalServerError()
        {
            // Arrange
            var printLabelDTO = new PrintLabelDTO
            {

            };
            _printLabelAppService.Setup(service => service.PrintLabelService(printLabelDTO)).Throws(new Exception("Some error"));

            // Act
            var result = _printLabelController.Post(printLabelDTO).Result as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);

        }

        [Test]
        public async Task GetAll_ReturnsOk()
        {
            // Arrange
            var expectedZebra = new List<ZebraConfigurationDTO> { new ZebraConfigurationDTO() };
            _printLabelAppService.Setup(service => service.GetZebraConfigurationsAsync()).ReturnsAsync(expectedZebra);

            // Act
            var result = await _printLabelController.GetAll() as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(expectedZebra, result.Value);
        }



        [Test]
        public async Task GetById_WhenValidId_ReturnsOk()
        {
            int zebraId = 1;
            var zebra = new ZebraConfigurationDTO();
            _printLabelAppService.Setup(service => service.GetZebraConfigurationByIdAsync(zebraId)).ReturnsAsync(zebra);

            // Act
            var result = await _printLabelController.GetById(zebraId) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(zebra, result.Value);
        }


        [Test]
        public async Task Post_WhenValidData_ReturnsOk2()
        {
            var entity = new ZebraConfigurationAddDTO
            {
                HostName = "localhost",
                Ip = "127.0.0.1",  
                NamePrinter = "printer01",
                PortType = "TCP",
                Port = 9100
            };
            int simulatedEntityId = 1;
            _printLabelAppService
                .Setup(service => service.AddZebraConfigurationAsync(It.IsAny<ZebraConfigurationAddDTO>()))
                .ReturnsAsync(simulatedEntityId);

            // Act
            var result = await _printLabelController.Post2(entity);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);


            var responseMessage = okResult.Value;
            Assert.NotNull(responseMessage);
        }



        [Test]
        public async Task Put_WhenValidData_ReturnsOk()
        {
            // Arrange
            int zebraId = 1;
            var zebraToEdit = new ZebraConfigurationAddDTO();
            _printLabelAppService.Setup(service => service.EditZebraConfigurationAsync(zebraId,zebraToEdit)).Returns(Task.CompletedTask);

            // Act
            var result = await _printLabelController.Put(zebraId,zebraToEdit) as ObjectResult;

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }



        [Test]
        public async Task Delete_WhenValidId_ReturnsOk()
        {
            // Arrange
            _printLabelAppService.Setup(service => service.DeleteZebraConfigurationAsync(1));

            // Act
            var result = await _printLabelController.Delete(1) as ObjectResult;

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }



    }
}

