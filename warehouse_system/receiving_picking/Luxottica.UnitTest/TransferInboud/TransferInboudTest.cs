using Luxottica.ApplicationServices.CameraAssignments;
using Luxottica.ApplicationServices.Cameras;
using Luxottica.ApplicationServices.DivertLines;
using Luxottica.ApplicationServices.JackpotLines;
using Luxottica.ApplicationServices.PhysicalMaps;
using Luxottica.ApplicationServices.Scanlogs;
using Luxottica.ApplicationServices.Shared.Dto.Camera;
using Luxottica.ApplicationServices.Shared.Dto.CameraAssignments;
using Luxottica.ApplicationServices.Shared.Dto.DivertLines;
using Luxottica.ApplicationServices.ToteInformations;
using Luxottica.Controllers.CameraAssignments;
using Luxottica.Controllers.Cameras;
using Luxottica.Controllers.DivertLines;
using Luxottica.Controllers.DivertOutboundLines;
using Luxottica.Controllers.Totes;
using Luxottica.Controllers.Totes.Luxottica.Controllers.PhysicalMaps;
using Luxottica.Core.Entities.CameraAssignments;
using Luxottica.Core.Entities.Cameras;
using Luxottica.Core.Entities.DivertLines;
using Luxottica.Core.Entities.ToteInformations;
using Luxottica.Models.Tote;
using Luxottica.Models.TransferInboud;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.UnitTest.TransferInboud
{
    public class TransferInboudTest
    {
        private Mock<ICameraAppService> _cameraAppServiceMock;
        private Mock<IDivertLineService> _lineServiceMock;
        private Mock<ICameraAssignmentService> _assignmentServiceMock;
        private Mock<IToteInformationAppService> _toteInformationAppServiceMock;
        private Mock<IJackpotLineAppService> _jackpotLineAppServiceMock;
        private Mock<IMapPhysicalAppService> _mapPhysicalAppServiceMock;
        private Mock<HttpClient> _http;
        private CameraController _cameraControllerMock;
        private DivertLineController _divertLineControllerMock;
        private CameraAssignmentController _cameraAssignmentControllerMock;
        private ToteInformationController _toteInformationControllerMock;
        private Mock<ILogger<CameraController>> _loggerCameraMock;
        private Mock<ILogger<DivertLineController>> _loggerDivertLineMock;
        private Mock<ILogger<CameraAssignmentController>> _loggerCameraAssigmentMock;
        private Mock<ILogger<ToteInformationController>> _loggerToteInformationMock;
        private Mock<IScanlogsAppService> _scanlogsAppService;
        [SetUp]
        public void Setup()
        {

            _cameraAppServiceMock = new Mock<ICameraAppService>();
            _lineServiceMock = new Mock<IDivertLineService>();
            _assignmentServiceMock = new Mock<ICameraAssignmentService>();
            _toteInformationAppServiceMock = new Mock<IToteInformationAppService>();
            _jackpotLineAppServiceMock = new Mock<IJackpotLineAppService>();
            _mapPhysicalAppServiceMock = new Mock<IMapPhysicalAppService>();
            _scanlogsAppService = new Mock<IScanlogsAppService>();

            _loggerToteInformationMock = new Mock<ILogger<ToteInformationController>>();
            _loggerCameraMock = new Mock<ILogger<CameraController>>();
            _loggerDivertLineMock = new Mock<ILogger<DivertLineController>>();
            _loggerCameraAssigmentMock = new Mock<ILogger<CameraAssignmentController>>();

        _http = new Mock<HttpClient>();
            _cameraControllerMock = new CameraController(_cameraAppServiceMock.Object,_loggerCameraMock.Object);
            _divertLineControllerMock = new DivertLineController(_lineServiceMock.Object, _loggerDivertLineMock.Object);
            _cameraAssignmentControllerMock = new CameraAssignmentController(_assignmentServiceMock.Object, _loggerCameraAssigmentMock.Object);
            _toteInformationControllerMock = new ToteInformationController(_toteInformationAppServiceMock.Object,_http.Object, _jackpotLineAppServiceMock.Object, _lineServiceMock.Object, _mapPhysicalAppServiceMock.Object,_loggerToteInformationMock.Object, _scanlogsAppService.Object);
            
        }

        [Test]
        public async Task CheckTote()
        {
            var newCamera = new CameraDTO { CamId = "CAM01" };
            var newDivertLine = new DivertLineAddDto { DivertLineValue = 1, StatusLine = true };
            var newCamAssign = new CameraAssignmentAddDto { CameraId = 1, DivertLineId = 1 };
            var newTote = new ToteInformationE
            {
                ToteLPN = "HG124",
                VirtualTote = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567",
                ZoneDivertId = 1,
                DivertStatus = null,
                LineCount = 0,
                TrackingId = 1,
            };
            var newTote1 = new ToteModel
            {
                toteLpn = "HG124",
                VirtualTote = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567",
                ZoneDivertId = 1,
                DivertStatus = null,
                LineCount = 0,
                TrackingId = 1,
            };
            var scanTote = new ToteScanInfoModel
            {

            };

            _cameraAppServiceMock.Setup(service => service.AddCameraAsync(newCamera));
            _lineServiceMock.Setup(service => service.AddDivertLineAsync(newDivertLine));
            _assignmentServiceMock.Setup(service => service.AddCameraAssignmentAsync(newCamAssign));
            _toteInformationAppServiceMock.Setup(service => service.AddToteInformationAsync(newTote));


            var camResult = await _cameraControllerMock.Post(newCamera);
            var divertLineResult = await _divertLineControllerMock.Post(newDivertLine);
            var camAssignResult = await _cameraAssignmentControllerMock.Post(newCamAssign);
            var toteInfoResult = await _toteInformationControllerMock.Post(newTote1);
        }
    }
}
