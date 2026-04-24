using Luxottica.ApplicationServices.DivertLines;
using Luxottica.ApplicationServices.PickingJackpotLines;
using Luxottica.ApplicationServices.Shared.Dto.DivertLines;
using Luxottica.Controllers.Commissioners;
using Luxottica.Controllers.PickingJackpotLines;
using Luxottica.Controllers.Totes.Luxottica.Controllers.PhysicalMaps;
using Luxottica.DataAccess;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.UnitTest.PickingJackpotLine
{
    [TestFixture]
    public class PickingJackpotLineTest
    {
        protected TestServer _server;
        private Mock<ILogger<PickingJackpotLineController>> _loggerMock;

        [OneTimeSetUp]
        public void Setup()
        {
            this._server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            _loggerMock = new Mock<ILogger<PickingJackpotLineController>>();
        }

        [Test]
        public async Task GetAll()
        {
            var appServices = _server.Host.Services.GetService<IPickingJackpotLineAppService>();
            var divertLineServices = _server.Host.Services.GetService<IDivertLineService>();
            var controller = new PickingJackpotLineController(appServices, _loggerMock.Object); 

            var divertLine = new DivertLineAddDto
            {
                DivertLineValue = 6,
                StatusLine = true
            };

            await divertLineServices.AddDivertLineAsync(divertLine);
            await controller.ChangePickingJackpot(6);

            var result = await controller.GetAll();

            Assert.NotNull(result);
            Assert.Pass();
        }

        [Test]
        public async Task Delete()
        {
            var appServices = _server.Host.Services.GetService<IPickingJackpotLineAppService>();
            var divertLineServices = _server.Host.Services.GetService<IDivertLineService>();
            var controller = new PickingJackpotLineController(appServices, _loggerMock.Object); 

            var divertLine = new DivertLineAddDto
            {
                DivertLineValue = 5,
                StatusLine = true
            };

            await divertLineServices.AddDivertLineAsync(divertLine);
            await controller.ChangePickingJackpot(5);

            await controller.Delete(1);

            var result = await controller.GetAll();

            Assert.IsEmpty(result);
            Assert.Pass();
        }

        [Test]
        public async Task ChangePickingJackpotSuccessful()
        {
            var appServices = _server.Host.Services.GetService<IPickingJackpotLineAppService>();
            var divertLineServices = _server.Host.Services.GetService<IDivertLineService>();
            var controller = new PickingJackpotLineController(appServices, _loggerMock.Object); 

            var divertLine = new DivertLineAddDto
            {
                DivertLineValue = 1,
                StatusLine = true
            };

            var divertLine2 = new DivertLineAddDto
            {
                DivertLineValue = 2,
                StatusLine = true
            };

            await divertLineServices.AddDivertLineAsync(divertLine);
            await divertLineServices.AddDivertLineAsync(divertLine2);

            var lines = await divertLineServices.GetDivertLineAsync();

            await controller.ChangePickingJackpot(1);

            var result = await controller.GetAll();

            await controller.ChangePickingJackpot(2);

            result = await controller.GetAll();
            Assert.That(result.First().DivertLineId, Is.EqualTo(2));
            Assert.Pass();
        }

        [Test]
        public async Task ChangePickingJackpotUnsuccessful()
        {
            var appServices = _server.Host.Services.GetService<IPickingJackpotLineAppService>();
            var divertLineServices = _server.Host.Services.GetService<IDivertLineService>();
            var controller = new PickingJackpotLineController(appServices, _loggerMock.Object); 

            var divertLine = new DivertLineAddDto
            {
                DivertLineValue = 3,
                StatusLine = true
            };

            var divertLine2 = new DivertLineAddDto
            {
                DivertLineValue = 4,
                StatusLine = false
            };

            await divertLineServices.AddDivertLineAsync(divertLine);
            await divertLineServices.AddDivertLineAsync(divertLine2);

            var lines = await divertLineServices.GetDivertLineAsync();

            await controller.ChangePickingJackpot(3);

            var picking = await controller.GetAll();

            var value = await controller.ChangePickingJackpot(4);

            var valueBadRequestObjectResult = (BadRequestObjectResult)value;

            Assert.NotNull(picking);
            Assert.That(value, Is.TypeOf<BadRequestObjectResult>());
            Assert.That(valueBadRequestObjectResult.StatusCode, Is.EqualTo(400));
            Assert.Pass();
        }
    }

}
