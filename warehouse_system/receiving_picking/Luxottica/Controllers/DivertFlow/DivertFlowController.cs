using Luxottica.ApplicationServices.Scanlogs;
using Luxottica.ApplicationServices.Shared.Dto.Scanlogs;
using Luxottica.ApplicationServices.Shared.Dto.ToteInformation;
using Luxottica.ApplicationServices.ToteInformations;
using Luxottica.Controllers.Totes.Luxottica.Controllers.PhysicalMaps;
using Luxottica.Models.DivertFlow;
using Luxottica.Models.TransferInboud;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Luxottica.Controllers.DivertFlow
{
    [Authorize]
    [Route("divert-flow")]
    [ApiController]
    public class DivertFlowController : ControllerBase
    {
        private readonly IToteInformationAppService _toteService;
        private readonly ILogger<DivertFlowController> _logger;
        private readonly IScanlogsAppService _scanlogsAppService;

        public DivertFlowController(IToteInformationAppService toteService, ILogger<DivertFlowController> logger, IScanlogsAppService scanlogsAppService)
        {
            _toteService = toteService;
            _logger = logger;
            _scanlogsAppService = scanlogsAppService;
        }
        [HttpPost("divert")]
        public async Task<IActionResult> Divert([FromBody] DivertModel DivertM)
        {
            try
            {
                var divert_code = new DivertCodeBundleModel();

                if (!ModelState.IsValid)
                {
                    _logger.LogError($"ERROR DIVERT IN DivertFlowController, Message: Error: JSON is not valid!");
                    return BadRequest("Error: JSON is not valid!");
                }

                if (string.IsNullOrWhiteSpace(DivertM.toteLpn))
                {
                    _logger.LogError($"ERROR DIVERT IN DivertFlowController, Message: The lenght LPN is empty, lpn is invalid: {DivertM.toteLpn}");
                    return BadRequest($"The lenght LPN is empty, lpn is invalid: {DivertM.toteLpn}");
                }
                if (string.IsNullOrWhiteSpace(DivertM.camId) || !DivertM.camId.StartsWith("cam", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogError($"ERROR DIVERT IN DivertFlowController, Message: The value should be: Cam, camera is invalid: {DivertM.camId}");
                    return BadRequest($"The value should be: Cam, camera is invalid: {DivertM.camId}");
                }
                if (DivertM.scannerNLaneWFull == 0 && DivertM.scannerNLaneWStatus == 1)
                {
                    if (DivertM.toteLpn == "?")
                    {
                        var isJackpotLine = await _toteService.IsJackpotLine(DivertM.camId);
                        if (isJackpotLine)
                        {
                            divert_code.divert_code = 2;
                            divert_code.tracking_id = DivertM.trakingId;
                            #region Scanlog
                            await _scanlogsAppService.AddScanlogAsync(new ScanlogsAddDto
                            {
                                ToteLPN = DivertM.toteLpn,
                                TrackingId = DivertM.trakingId,
                                Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                                CamId = DivertM.camId,
                                DivertCode = divert_code.divert_code,
                                Info = "Tote diverted to Picking Jackpot"
                            });
                            #endregion
                            return new OkObjectResult(divert_code);
                        }
                    }

                    int divertResult = await _toteService.DivertTote(DivertM.toteLpn, DivertM.camId, DivertM.trakingId);

                    divert_code.divert_code = divertResult;
                    divert_code.tracking_id = DivertM.trakingId;
                    return new OkObjectResult(divert_code);
                }
                else
                {
                    divert_code.divert_code = 99;
                    divert_code.tracking_id = DivertM.trakingId;
                    #region Scanlog
                    await _scanlogsAppService.AddScanlogAsync(new ScanlogsAddDto
                    {
                        ToteLPN = DivertM.toteLpn,
                        TrackingId = DivertM.trakingId,
                        Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                        CamId = DivertM.camId,
                        DivertCode = divert_code.divert_code,
                        Info = "The Tote couldn't divert because Divertlane is full."
                    });
                    #endregion
                    return new OkObjectResult(divert_code);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DIVERT IN DivertFlowController, Message {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error has occurred in the process" });
            }
        }


        [HttpPost("confirmation")]
        public async Task<IActionResult> Confirmation([FromBody] ConfirmationModel confirmationVM)
        {
            var timestampPLC = DateTime.Now.ToString("yyyyMMddHHmmssfff");

            var patron = @"^\d{17}$";
            if (!ModelState.IsValid || confirmationVM.divertCode != 100 || !Regex.IsMatch(timestampPLC, patron))
            {
                _logger.LogError($"ERROR CONFIRMATION IN DivertFlowController, Message: Error: JSON is not valid!");
                return StatusCode(500, "Error: JSON is not valid!");
            }

            try
            {
                var result = await _toteService.DivertConfirm(timestampPLC, confirmationVM.trakingId, confirmationVM.camId);
                if (result == 2)
                {
                    return Ok();
                }
                else
                {
                    _logger.LogError($"ERROR CONFIRMATION IN DivertFlowController, Message: Tote divert confirm incomplete");
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error: Tote divert confirm incomplete" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR CONFIRMATION IN DivertFlowController, Message {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while confirm tote." });
            }
        }

        [HttpPost("confirmation/Cam14")]
        public async Task<IActionResult> ConfirmationCam14([FromBody] ConfirmationModel confirmationVM)
        {
            var timestampPLC = DateTime.Now.ToString("yyyyMMddHHmmssfff");

            var patron = @"^\d{17}$";
            if (!ModelState.IsValid || confirmationVM.divertCode != 100 || !Regex.IsMatch(timestampPLC, patron))
            {
                return StatusCode(500, "Error: JSON is not valid!");
            }

            try
            {
                var result = await _toteService.DivertConfirmCam14(timestampPLC, confirmationVM.trakingId, confirmationVM.camId);
                if (result == 2)
                {
                    return Ok();
                }
                else
                {
                    _logger.LogError($"ERROR ConfirmationCam14 IN DivertFlowController, Message: Error: Tote divert confirm incomplete");
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error: Tote divert confirm incomplete" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR ConfirmationCam14 IN DivertFlowController, Message {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while confirm tote." });
            }
        }

        [HttpPost("confirmation/Cam15")]
        public async Task<IActionResult> ConfirmationCam15([FromBody] ConfirmationModel confirmationVM)
        {
            var timestampPLC = DateTime.Now.ToString("yyyyMMddHHmmssfff");

            var patron = @"^\d{17}$";
            if (!ModelState.IsValid || confirmationVM.divertCode != 100 || !Regex.IsMatch(timestampPLC, patron))
            {
                _logger.LogError($"ERROR ConfirmationCam15 IN DivertFlowController, Message: Error: Error: JSON is not valid!");
                return StatusCode(500, "Error: JSON is not valid!");
            }

            try
            {
                var result = await _toteService.DivertConfirmCam15(timestampPLC, confirmationVM.trakingId, confirmationVM.camId);
                if (result == 2)
                {
                    return Ok();
                }
                else
                {
                    _logger.LogError($"ERROR ConfirmationCam15 IN DivertFlowController, Message: Error: Tote divert confirm incomplete");
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error: Tote divert confirm incomplete" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR ConfirmationCam15 IN DivertFlowController, Message {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while confirm tote." });
            }
        }

        [HttpPost("confirmation/Cam16")]
        public async Task<IActionResult> ConfirmationCam16([FromBody] ConfirmationModel confirmationVM)
        {
            var timestampPLC = DateTime.Now.ToString("yyyyMMddHHmmssfff");

            var patron = @"^\d{17}$";
            if (!ModelState.IsValid || confirmationVM.divertCode != 100 || !Regex.IsMatch(timestampPLC, patron))
            {
                _logger.LogError($"ERROR ConfirmationCam16 IN DivertFlowController, Message: Error: JSON is not valid!");
                return StatusCode(500, "Error: JSON is not valid!");
            }

            try
            {
                var result = await _toteService.DivertConfirmCam16(timestampPLC, confirmationVM.trakingId, confirmationVM.camId);
                if (result == 2)
                {
                    return Ok();
                }
                else
                {
                    _logger.LogError($"ERROR ConfirmationCam16 IN DivertFlowController, Message: Error: Tote divert confirm incomplete");
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error: Tote divert confirm incomplete" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR ConfirmationCam16 IN DivertFlowController, Message {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while confirm tote." });
            }
        }

        [HttpPost("confirmation/Cam17")]
        public async Task<IActionResult> ConfirmationCam17([FromBody] ConfirmationModel confirmationVM)
        {
            var timestampPLC = DateTime.Now.ToString("yyyyMMddHHmmssfff");

            var patron = @"^\d{17}$";
            if (!ModelState.IsValid || confirmationVM.divertCode != 100 || !Regex.IsMatch(timestampPLC, patron))
            {
                _logger.LogError($"ERROR ConfirmationCam17 IN DivertFlowController, Message: Error: JSON is not valid!");
                return StatusCode(500, "Error: JSON is not valid!");
            }

            try
            {
                var result = await _toteService.DivertConfirmCam17(timestampPLC, confirmationVM.trakingId, confirmationVM.camId);
                if (result == 2)
                {
                    return Ok();
                }
                else
                {
                    _logger.LogError($"ERROR ConfirmationCam17 IN DivertFlowController, Message: Error: Tote divert confirm incomplete");
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error: Tote divert confirm incomplete" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR ConfirmationCam17 IN DivertFlowController, Message {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while confirm tote." });
            }
        }

        [HttpPost("divert/singleTote")]
        public async Task<IActionResult> DivertSingleTote([FromBody] DivertModel DivertM)
        {
            try
            {
                var divert_code = new DivertCodeBundleModel();

                if (!ModelState.IsValid)
                {
                    _logger.LogError($"ERROR DIVERT Single Tote IN DivertFlowController, Message: Error: JSON is not valid!");
                    return BadRequest("Error: JSON is not valid!");
                }
                if (string.IsNullOrWhiteSpace(DivertM.toteLpn))
                {
                    _logger.LogError($"ERROR DIVERT Single Tote IN DivertFlowController, Message: The lenght LPN is empty, lpn is invalid: {DivertM.toteLpn}");
                    return BadRequest($"The lenght LPN is empty, lpn is invalid: {DivertM.toteLpn}");
                }
                if (string.IsNullOrWhiteSpace(DivertM.camId) || !DivertM.camId.StartsWith("cam", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogError($"ERROR DIVERT Single Tote IN DivertFlowController, Message: The value should be: Cam, camera is invalid: {DivertM.camId}");
                    return BadRequest($"The value should be: Cam, camera is invalid: {DivertM.camId}");
                }
                if (DivertM.scannerNLaneWFull == 0 && DivertM.scannerNLaneWStatus == 1)
                {
                    if (DivertM.toteLpn == "?")
                    {
                        var isJackpotLine = await _toteService.IsPickingJackpotLine(DivertM.camId);
                        if (isJackpotLine)
                        {
                            divert_code.divert_code = 2;
                            divert_code.tracking_id = DivertM.trakingId;
                            #region Scanlog
                            await _scanlogsAppService.AddScanlogAsync(new ScanlogsAddDto
                            {
                                ToteLPN = DivertM.toteLpn,
                                TrackingId = DivertM.trakingId,
                                Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                                CamId = DivertM.camId,
                                DivertCode = divert_code.divert_code,
                                Info = "Tote diverted to Picking Jackpot"
                            });
                            #endregion
                            return new OkObjectResult(divert_code);
                        }
                    }

                    var divertResult = await _toteService.DivertToteSingle(DivertM.toteLpn, DivertM.camId, DivertM.trakingId);

                    divert_code.divert_code = divertResult;
                    divert_code.tracking_id = DivertM.trakingId;
                    return new OkObjectResult(divert_code);
                }
                else
                {
                    divert_code.divert_code = 99;
                    divert_code.tracking_id = DivertM.trakingId;
                    #region Scanlog
                    await _scanlogsAppService.AddScanlogAsync(new ScanlogsAddDto
                    {
                        ToteLPN = DivertM.toteLpn,
                        TrackingId = DivertM.trakingId,
                        Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                        CamId = DivertM.camId,
                        DivertCode = divert_code.divert_code,
                        Info = "Totes singles at limited capacity"
                    });
                    #endregion
                    return new OkObjectResult(divert_code);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DivertSingleTote IN DivertFlowController, Message {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error has occurred in the process" });
            }
        }

        [HttpPost("divert/multiTote")]
        public async Task<IActionResult> DivertMultiTote([FromBody] DivertModel DivertM)
        {
            try
            {
                var divert_code = new DivertCodeBundleModel();

                if (!ModelState.IsValid)
                {
                    _logger.LogError($"ERROR DIVERT Multi Tote IN DivertFlowController, Message: Error: JSON is not valid!");
                    return BadRequest("Error: JSON is not valid!");
                }
                if (string.IsNullOrWhiteSpace(DivertM.toteLpn))
                {
                    _logger.LogError($"ERROR DIVERT Multi Tote IN DivertFlowController, Message: The lenght LPN is empty, lpn is invalid: {DivertM.toteLpn}");
                    return BadRequest($"The lenght LPN is empty, lpn is invalid: {DivertM.toteLpn}");
                }
                if (string.IsNullOrWhiteSpace(DivertM.camId) || !DivertM.camId.StartsWith("cam", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogError($"ERROR DIVERT Multi Tote IN DivertFlowController, Message: The value should be: Cam, camera is invalid: {DivertM.camId}");
                    return BadRequest($"The value should be: Cam, camera is invalid: {DivertM.camId}");
                }
                if (DivertM.scannerNLaneWFull == 0 && DivertM.scannerNLaneWStatus == 1)
                {
                    if (DivertM.toteLpn == "?")
                    {
                        var isJackpotLine = await _toteService.IsPickingJackpotLine(DivertM.camId);
                        if (isJackpotLine)
                        {
                            divert_code.divert_code = 2;
                            divert_code.tracking_id = DivertM.trakingId;
                            #region Scanlog
                            await _scanlogsAppService.AddScanlogAsync(new ScanlogsAddDto
                            {
                                ToteLPN = DivertM.toteLpn,
                                TrackingId = DivertM.trakingId,
                                Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                                CamId = DivertM.camId,
                                DivertCode = divert_code.divert_code,
                                Info = "Tote diverted to Picking Jackpot"
                            });
                            #endregion
                            return new OkObjectResult(divert_code);
                        }
                    }

                    var divertResult = await _toteService.DivertToteMulti(DivertM.toteLpn, DivertM.camId, DivertM.trakingId);

                    divert_code.divert_code = divertResult;
                    divert_code.tracking_id = DivertM.trakingId;
                    return new OkObjectResult(divert_code);
                }
                else
                {
                    divert_code.divert_code = 99;
                    divert_code.tracking_id = DivertM.trakingId;
                    #region Scanlog
                    await _scanlogsAppService.AddScanlogAsync(new ScanlogsAddDto
                    {
                        ToteLPN = DivertM.toteLpn,
                        TrackingId = DivertM.trakingId,
                        Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                        CamId = DivertM.camId,
                        DivertCode = divert_code.divert_code,
                        Info = "The Tote couldn't divert because Lane is full."
                    });
                    #endregion
                    return new OkObjectResult(divert_code);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DivertMultiTote IN DivertFlowController, Message {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error has occurred in the process" });
            }
        }

        [HttpPost("divert/multiToteCam14")]
        public async Task<IActionResult> DivertMultiToteCam14([FromBody] DivertModel DivertM)
        {
            try
            {
                var divert_code = new DivertCodeBundleModel();

                if (!ModelState.IsValid)
                {
                    _logger.LogError($"ERROR DIVERT Multi Tote CAM14 IN DivertFlowController, Message: Error: JSON is not valid!");
                    return BadRequest("Error: JSON is not valid!");
                }
                if (string.IsNullOrWhiteSpace(DivertM.toteLpn))
                {
                    _logger.LogError($"ERROR DIVERT Multi Tote CAM14 IN DivertFlowController, Message: The lenght LPN is empty, lpn is invalid: {DivertM.toteLpn}");
                    return BadRequest($"The lenght LPN is empty, lpn is invalid: {DivertM.toteLpn}");
                }
                if (string.IsNullOrWhiteSpace(DivertM.camId) || !DivertM.camId.StartsWith("cam", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogError($"ERROR DIVERT Multi Tote CAM14 IN DivertFlowController, Message: The value should be: Cam, camera is invalid: {DivertM.camId}");
                    return BadRequest($"The value should be: Cam, camera is invalid: {DivertM.camId}");
                }
                if (DivertM.scannerNLaneWFull == 0 && DivertM.scannerNLaneWStatus == 1)
                {
                    if (DivertM.toteLpn == "?")
                    {
                        var isJackpotLine = await _toteService.IsPickingJackpotLine(DivertM.camId);
                        if (isJackpotLine)
                        {
                            divert_code.divert_code = 2;
                            divert_code.tracking_id = DivertM.trakingId;
                            #region Scanlog
                            await _scanlogsAppService.AddScanlogAsync(new ScanlogsAddDto
                            {
                                ToteLPN = DivertM.toteLpn,
                                TrackingId = DivertM.trakingId,
                                Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                                CamId = DivertM.camId,
                                DivertCode = divert_code.divert_code,
                                Info = "Tote diverted to Picking Jackpot"
                            });
                            #endregion
                            return new OkObjectResult(divert_code);
                        }
                    }

                    var divertResult = await _toteService.DivertMultiToteCam14(DivertM.toteLpn, DivertM.camId, DivertM.trakingId);

                    divert_code.divert_code = divertResult;
                    divert_code.tracking_id = DivertM.trakingId;
                    return new OkObjectResult(divert_code);
                }
                else
                {
                    divert_code.divert_code = 99;
                    divert_code.tracking_id = DivertM.trakingId;
                    #region Scanlog
                    await _scanlogsAppService.AddScanlogAsync(new ScanlogsAddDto
                    {
                        ToteLPN = DivertM.toteLpn,
                        TrackingId = DivertM.trakingId,
                        Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                        CamId = DivertM.camId,
                        DivertCode = divert_code.divert_code,
                        Info = "The Tote couldn't divert because Lane is full."
                    });
                    #endregion
                    return new OkObjectResult(divert_code);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DivertMultiToteCam14 IN DivertFlowController, Message {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error has occurred in the process" });
            }
        }

        [HttpPost("divert/multiToteCam15")]
        public async Task<IActionResult> DivertMultiToteCam15([FromBody] DivertModel DivertM)
        {
            try
            {
                var divert_code = new DivertCodeBundleModel();

                if (!ModelState.IsValid)
                {
                    _logger.LogError($"ERROR DIVERT Multi Tote CAM15 IN DivertFlowController, Message: Error: JSON is not valid!");
                    return BadRequest("Error: JSON is not valid!");
                }
                if (string.IsNullOrWhiteSpace(DivertM.toteLpn))
                {
                    _logger.LogError($"ERROR DIVERT Multi Tote CAM15 IN DivertFlowController, Message: The lenght LPN is empty, lpn is invalid: {DivertM.toteLpn}");
                    return BadRequest($"The lenght LPN is empty, lpn is invalid: {DivertM.toteLpn}");
                }
                if (string.IsNullOrWhiteSpace(DivertM.camId) || !DivertM.camId.StartsWith("cam", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogError($"ERROR DIVERT Multi Tote CAM15 IN DivertFlowController, Message: The value should be: Cam, camera is invalid: {DivertM.camId}");
                    return BadRequest($"The value should be: Cam, camera is invalid: {DivertM.camId}");
                }
                if (DivertM.scannerNLaneWFull == 0 && DivertM.scannerNLaneWStatus == 1)
                {
                    if (DivertM.toteLpn == "?")
                    {
                        var isJackpotLine = await _toteService.IsPickingJackpotLine(DivertM.camId);
                        if (isJackpotLine)
                        {
                            divert_code.divert_code = 2;
                            divert_code.tracking_id = DivertM.trakingId;
                            #region Scanlog
                            await _scanlogsAppService.AddScanlogAsync(new ScanlogsAddDto
                            {
                                ToteLPN = DivertM.toteLpn,
                                TrackingId = DivertM.trakingId,
                                Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                                CamId = DivertM.camId,
                                DivertCode = divert_code.divert_code,
                                Info = "Tote diverted to Picking Jackpot"
                            });
                            #endregion
                            return new OkObjectResult(divert_code);
                        }
                    }

                    var divertResult = await _toteService.DivertMultiToteCam15(DivertM.toteLpn, DivertM.camId, DivertM.trakingId);

                    divert_code.divert_code = divertResult;
                    divert_code.tracking_id = DivertM.trakingId;
                    return new OkObjectResult(divert_code);
                }
                else
                {
                    divert_code.divert_code = 99;
                    divert_code.tracking_id = DivertM.trakingId;
                    #region Scanlog
                    await _scanlogsAppService.AddScanlogAsync(new ScanlogsAddDto
                    {
                        ToteLPN = DivertM.toteLpn,
                        TrackingId = DivertM.trakingId,
                        Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                        CamId = DivertM.camId,
                        DivertCode = divert_code.divert_code,
                        Info = "The Tote couldn't divert because Lane is full."
                    });
                    #endregion
                    return new OkObjectResult(divert_code);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DivertMultiToteCam15 IN DivertFlowController, Message {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error has occurred in the process" });
            }
        }

        [HttpPost("divert/multiToteCam16")]
        public async Task<IActionResult> DivertMultiToteCam16([FromBody] DivertModel DivertM)
        {
            try
            {
                var divert_code = new DivertCodeBundleModel();

                if (!ModelState.IsValid)
                {
                    _logger.LogError($"ERROR DIVERT Multi Tote CAM16 IN DivertFlowController, Message: Error: JSON is not valid!");
                    return BadRequest("Error: JSON is not valid!");
                }
                if (string.IsNullOrWhiteSpace(DivertM.toteLpn))
                {
                    _logger.LogError($"ERROR DIVERT Multi Tote CAM16 IN DivertFlowController, Message: The lenght LPN is empty, lpn is invalid: {DivertM.toteLpn}");
                    return BadRequest($"The lenght LPN is empty, lpn is invalid: {DivertM.toteLpn}");
                }
                if (string.IsNullOrWhiteSpace(DivertM.camId) || !DivertM.camId.StartsWith("cam", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogError($"ERROR DIVERT Multi Tote CAM16 IN DivertFlowController, Message: The value should be: Cam, camera is invalid: {DivertM.camId}");
                    return BadRequest($"The value should be: Cam, camera is invalid: {DivertM.camId}");
                }
                if (DivertM.scannerNLaneWFull == 0 && DivertM.scannerNLaneWStatus == 1)
                {
                    if (DivertM.toteLpn == "?")
                    {
                        var isJackpotLine = await _toteService.IsPickingJackpotLine(DivertM.camId);
                        if (isJackpotLine)
                        {
                            divert_code.divert_code = 2;
                            divert_code.tracking_id = DivertM.trakingId;
                            #region Scanlog
                            await _scanlogsAppService.AddScanlogAsync(new ScanlogsAddDto
                            {
                                ToteLPN = DivertM.toteLpn,
                                TrackingId = DivertM.trakingId,
                                Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                                CamId = DivertM.camId,
                                DivertCode = divert_code.divert_code,
                                Info = "Tote diverted to Picking Jackpot"
                            });
                            #endregion
                            return new OkObjectResult(divert_code);
                        }
                    }

                    var divertResult = await _toteService.DivertMultiToteCam16(DivertM.toteLpn, DivertM.camId, DivertM.trakingId);

                    divert_code.divert_code = divertResult;
                    divert_code.tracking_id = DivertM.trakingId;
                    return new OkObjectResult(divert_code);
                }
                else
                {
                    divert_code.divert_code = 99;
                    divert_code.tracking_id = DivertM.trakingId;
                    #region Scanlog
                    await _scanlogsAppService.AddScanlogAsync(new ScanlogsAddDto
                    {
                        ToteLPN = DivertM.toteLpn,
                        TrackingId = DivertM.trakingId,
                        Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                        CamId = DivertM.camId,
                        DivertCode = divert_code.divert_code,
                        Info = "The Tote couldn't divert because Lane is full."
                    });
                    #endregion
                    return new OkObjectResult(divert_code);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DivertMultiToteCam16 IN DivertFlowController, Message {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error has occurred in the process" });
            }
        }

        [HttpPost("divert/multiToteCam17")]
        public async Task<IActionResult> DivertMultiToteCam17([FromBody] DivertModel DivertM)
        {
            try
            {
                var divert_code = new DivertCodeBundleModel();

                if (!ModelState.IsValid)
                {
                    _logger.LogError($"ERROR DIVERT Multi Tote CAM17 IN DivertFlowController, Message: Error: JSON is not valid!");
                    return BadRequest("Error: JSON is not valid!");
                }
                if (string.IsNullOrWhiteSpace(DivertM.toteLpn))
                {
                    _logger.LogError($"ERROR DIVERT Multi Tote CAM17 IN DivertFlowController, Message: The lenght LPN is empty, lpn is invalid: {DivertM.toteLpn}");
                    return BadRequest($"The lenght LPN is empty, lpn is invalid: {DivertM.toteLpn}");
                }
                if (string.IsNullOrWhiteSpace(DivertM.camId) || !DivertM.camId.StartsWith("cam", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogError($"ERROR DIVERT Multi Tote CAM17 IN DivertFlowController, Message: The value should be: Cam, camera is invalid: {DivertM.camId}");
                    return BadRequest($"The value should be: Cam, camera is invalid: {DivertM.camId}");
                }
                if (DivertM.scannerNLaneWFull == 0 && DivertM.scannerNLaneWStatus == 1)
                {
                    if (DivertM.toteLpn == "?")
                    {
                        var isJackpotLine = await _toteService.IsPickingJackpotLine(DivertM.camId);
                        if (isJackpotLine)
                        {
                            divert_code.divert_code = 2;
                            divert_code.tracking_id = DivertM.trakingId;
                            #region Scanlog
                            await _scanlogsAppService.AddScanlogAsync(new ScanlogsAddDto
                            {
                                ToteLPN = DivertM.toteLpn,
                                TrackingId = DivertM.trakingId,
                                Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                                CamId = DivertM.camId,
                                DivertCode = divert_code.divert_code,
                                Info = "Tote diverted to Picking Jackpot"
                            });
                            #endregion
                            return new OkObjectResult(divert_code);
                        }
                    }

                    var divertResult = await _toteService.DivertMultiToteCam17(DivertM.toteLpn, DivertM.camId, DivertM.trakingId);

                    divert_code.divert_code = divertResult;
                    divert_code.tracking_id = DivertM.trakingId;
                    return new OkObjectResult(divert_code);
                }
                else
                {
                    divert_code.divert_code = 99;
                    divert_code.tracking_id = DivertM.trakingId;
                    #region Scanlog
                    await _scanlogsAppService.AddScanlogAsync(new ScanlogsAddDto
                    {
                        ToteLPN = DivertM.toteLpn,
                        TrackingId = DivertM.trakingId,
                        Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                        CamId = DivertM.camId,
                        DivertCode = divert_code.divert_code,
                        Info = "The Tote couldn't divert because Lane is full."
                    });
                    #endregion
                    return new OkObjectResult(divert_code);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DivertMultiToteCam17 IN DivertFlowController, Message {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error has occurred in the process" });
            }
        }

        [HttpPost("divert/TotesCam11")]
        public async Task<IActionResult> DivertTotesInCam11([FromBody] DivertModel DivertM)
        {
            try
            {
                var divert_code = new DivertCodeBundleModel();

                if (!ModelState.IsValid)
                {
                    _logger.LogError($"ERROR DIVERT TOTES CAM11 IN DivertFlowController, Message: Error: JSON is not valid!");
                    return BadRequest("Error: JSON is not valid!");
                }
                if (string.IsNullOrWhiteSpace(DivertM.toteLpn))
                {
                    _logger.LogError($"ERROR DIVERT TOTES CAM11 IN DivertFlowController, Message: The lenght LPN is empty, lpn is invalid: {DivertM.toteLpn}");
                    return BadRequest($"The lenght LPN is empty, lpn is invalid: {DivertM.toteLpn}");
                }
                if (string.IsNullOrWhiteSpace(DivertM.camId) || !DivertM.camId.StartsWith("cam", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogError($"ERROR DIVERT TOTES CAM11 IN DivertFlowController, Message: The value should be: Cam, camera is invalid: {DivertM.camId}");
                    return BadRequest($"The value should be: Cam, camera is invalid: {DivertM.camId}");
                }
                if (DivertM.scannerNLaneWFull == 0 && DivertM.scannerNLaneWStatus == 1)
                {
                    if (DivertM.toteLpn == "?")
                    {
                        var isJackpotLine = await _toteService.IsPickingJackpotLine(DivertM.camId);
                        if (isJackpotLine)
                        {
                            divert_code.divert_code = 2;
                            divert_code.tracking_id = DivertM.trakingId;
                            #region Scanlog
                            await _scanlogsAppService.AddScanlogAsync(new ScanlogsAddDto
                            {
                                ToteLPN = DivertM.toteLpn,
                                TrackingId = DivertM.trakingId,
                                Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                                CamId = DivertM.camId,
                                DivertCode = divert_code.divert_code,
                                Info = "Tote diverted to Picking Jackpot"
                            });
                            #endregion
                            return new OkObjectResult(divert_code);
                        }
                    }

                    var divertResult = await _toteService.DivertTotesInCam11(DivertM.toteLpn, DivertM.camId, DivertM.trakingId);

                    divert_code.divert_code = divertResult;
                    divert_code.tracking_id = DivertM.trakingId;
                    return new OkObjectResult(divert_code);
                }
                else
                {
                    divert_code.divert_code = 99;
                    divert_code.tracking_id = DivertM.trakingId;
                    #region Scanlog
                    await _scanlogsAppService.AddScanlogAsync(new ScanlogsAddDto
                    {
                        ToteLPN = DivertM.toteLpn,
                        TrackingId = DivertM.trakingId,
                        Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                        CamId = DivertM.camId,
                        DivertCode = divert_code.divert_code,
                        Info = "Picking Totes at limited capacity"
                    });
                    #endregion
                    return new OkObjectResult(divert_code);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DivertTotesInCam11 IN DivertFlowController, Message {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error has occurred in the process" });
            }
        }

    }
}
