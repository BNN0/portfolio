using LuxotticaSorting.ApplicationServices.MappingSorter;
using LuxotticaSorting.ApplicationServices.Shared.DTO.BoxTypes;
using LuxotticaSorting.ApplicationServices.Shared.DTO.MappingSorters;
using LuxotticaSorting.Controllers.MappingSorters;
using LuxotticaSorting.Core.BoxTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxxoticaSorting.Unitest.MappingSorters
{
    public class MappingSorterTest
    {
        private MappingSorterController _controller;
        private Mock<IMappingSorterAppService> _service;
        private Mock<ILogger<MappingSorterController>> _logger;

        [SetUp]
        public void Setup()
        {
            _service = new Mock<IMappingSorterAppService>();
            _logger = new Mock<ILogger<MappingSorterController>>();
            _controller = new MappingSorterController(_service.Object, _logger.Object);
        }

        [Test]
        public async Task GetAll()
        {
            // Arrange
            _service.Setup(s => s.GetMappingSorterAsync()).ReturnsAsync(new List<MappingSorterDto>());

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task GetById()
        {
            // Arrange
            int id = 1;
            _service.Setup(s => s.GetMappingSorterByIdAsync(id)).ReturnsAsync(new MappingSorterDto());

            // Act
            var result = await _controller.GetById(id);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }
        
        [Test]
        public async Task Post()
        {
            _service.Setup(service => service.AddMappingSorterAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Post() as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task Put()
        {
            // Arrange
            int id = 1;
            var entity = new MappingSorterAddDto
            {
                LogisticAgentId = 2,
                CarrierCodeId = "1",
                BoxTypeId = "1"
            };

            _service.Setup(s => s.EditMappingSorterAsync(id, entity)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Put(id, entity) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }
        /*
        [Test]
        public async Task Delete()
        {
            // Arrange
            int id = 1;

            _service.Setup(s => s.DeleteMappingSorterAsync(id)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(id) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }
        */
        [Test]
        public async Task GetAllCombinedData()
        {
            // Arrange
            _service.Setup(s => s.GetCombinedDataAsync()).ReturnsAsync(new List<MappingSorterGetAllDto>());

            // Act
            var result = await _controller.GetAllCombinedData();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }
    }
}
