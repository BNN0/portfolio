using AutoMapper;
using LuxotticaSorting.ApplicationServices.DivertBox;
using LuxotticaSorting.ApplicationServices.DivertLanes;
using LuxotticaSorting.ApplicationServices.NewBoxs;
using LuxotticaSorting.ApplicationServices.RoutingV10;
using LuxotticaSorting.ApplicationServices.ScanlogSortings;
using LuxotticaSorting.ApplicationServices.Shared.Dto.RoutingV10S;
using LuxotticaSorting.ApplicationServices.Shared.DTO.RoutingV10S;
using LuxotticaSorting.Controllers.NewBoxs;
using LuxotticaSorting.Controllers.RoutingV10S;
using LuxotticaSorting.Core.WCSRoutingV10;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LuxotticaSorting.Controllers.DivertBox
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class DivertBoxController : ControllerBase
    {
        private readonly IDivertBoxAppService _divertBoxAppService;
        private readonly IDivertLanesAppService _divertLanesAppService;
        private readonly ILogger<DivertBoxController> _logger;
        private readonly IRoutingAppService _routingAppService;
        private readonly RoutingV10SController _routingV10SController;
        public DivertBoxController(IDivertBoxAppService divertBoxAppService, ILogger<DivertBoxController> logger, IDivertLanesAppService divertLanesAppService, IRoutingAppService routingAppService)
        {
            _divertBoxAppService = divertBoxAppService;
            _logger = logger;
            _divertLanesAppService = divertLanesAppService;
            _routingAppService = routingAppService;
        }

        // POST api/<DivertBoxController>
        [HttpPost("Destination")]
        public async Task<IActionResult> Destination([FromBody] DestinationReqDto model)
        {
            try
            {
                ErrorDestinationDto errorDestinationDto = new ErrorDestinationDto();

                if (model.BoxId == "?" || model.BoxId == "111111") 
                {
                    var idH = (await _divertLanesAppService.GetDivertLanesAsync()).Find(item => item.DivertLanes == 32) ?? throw new Exception("No divert lane hospital found");
                    var statusHospital = await _divertLanesAppService.GetDivertLaneByIdAsync(idH.Id);

                    if (statusHospital.Status == true && statusHospital.Full == false)
                    {
                        errorDestinationDto = new ErrorDestinationDto
                        {
                            Message = "Error in BoxId redirected to hospital",
                            TrackingId = model.TrackingId,
                            DivertCode = 32
                        };
                        await _divertBoxAppService.RegisterHospital(model.BoxId,32);
                    }
                    else
                    {
                        errorDestinationDto = new ErrorDestinationDto
                        {
                            Message = "Hospital offline or full",
                            TrackingId = model.TrackingId,
                            DivertCode = 99
                        };
                        await _divertBoxAppService.RegisterHospital(model.BoxId,99);
                    }

                    return BadRequest(errorDestinationDto);
                }

                
                var routingSuccess = await _routingAppService.GetOrdersBoxIdSAPInformation(model.BoxId, model.TrackingId);

                //WCSRoutingV10 routingV10 = new WCSRoutingV10
                //{
                //    Id = routingSuccess.Id,
                //    BoxId = routingSuccess.BoxId,
                //    BoxType = routingSuccess.BoxType,
                //    CarrierCode = routingSuccess.CarrierCode,
                //    LogisticAgent = routingSuccess.LogisticAgent,
                //    ConfirmationNumber = routingSuccess.ConfirmationNumber,
                //    ContainerId = routingSuccess.ContainerId,
                //    ContainerType = routingSuccess.ContainerType,
                //    Qty = (int)routingSuccess.Qty,
                //    DivertLane = routingSuccess.DivertLane,
                //    CurrentTs = routingSuccess.CurrentTs,
                //    Status = routingSuccess.Status,
                //    SAPSystem = routingSuccess.SAPSystem,
                //    DivertTs = routingSuccess.DivertTs,
                //    TrackingId = routingSuccess.TrackingId,
                //    Count = routingSuccess.Count,
                //};

                //await _divertBoxAppService.RegisterAddScanLog(routingV10);

                if(routingSuccess != null)
                {
                    DivertBoxReqDto divertBoxReqDto = new DivertBoxReqDto 
                    {
                        BoxId = model.BoxId,
                        TrackingId = model.TrackingId
                    };
                    var divertResult = await this.Post(divertBoxReqDto) as ObjectResult;

                    if(divertResult.StatusCode == 200)
                    {
                        var divertResultValue = divertResult.Value;
                        return new OkObjectResult(divertResultValue);

                    }
                    _logger.LogError("An error occurred while Divert Method");
                    errorDestinationDto = new ErrorDestinationDto
                    {
                        Message = "Divert Method not successful",
                        TrackingId = model.TrackingId,
                        DivertCode = 99
                    };
                    
                    return BadRequest(errorDestinationDto);
                }
                _logger.LogError("An error occurred while Register method");
                errorDestinationDto = new ErrorDestinationDto
                {
                    Message = "Divert Method not successful",
                    TrackingId = model.TrackingId,
                    DivertCode = 99
                };
                return BadRequest(errorDestinationDto);
            }
            catch (Exception ex)
            {
                ErrorDestinationDto errorDestinationDto = new ErrorDestinationDto();
                _logger.LogError("An error occurred in Destination method");
                errorDestinationDto.Message = ex.Message;
                errorDestinationDto.TrackingId = model.TrackingId;
                return BadRequest(errorDestinationDto);
            }
        }


        // POST api/<DivertBoxController>
        [HttpPost("Divert")]
        public async Task<IActionResult> Post([FromBody] DivertBoxReqDto model)
        {
            var DivertBoxResp = new DivertBoxRespDto();
            try
            {
                if(ModelState.IsValid)
                {
                    var divertResult = await _divertBoxAppService.DivertBox(model.BoxId, model.TrackingId);
                    //await _divertBoxAppService.RegisterAddScanLog(routingV10);
                    return new OkObjectResult(divertResult);
                }
                return BadRequest("JSON is not valid");
            }
            catch (Exception)
            {
                _logger.LogError("An error occurred in DivertBoxController while confirm tote");
                return StatusCode(500, "An error occurred while confirm tote.");
            }
        }


        [HttpPost("Confirmation")]
        public async Task<IActionResult> Confirmation([FromBody] ConfirmBoxReqDto confirmation)
        {
            var timestampPLC = DateTime.Now.ToString("yyyyMMddHHmmssfff");

            var patron = @"^\d{17}$";
            if (!ModelState.IsValid || confirmation.DivertCode != 100 || !Regex.IsMatch(timestampPLC, patron))
            {
                _logger.LogError($"Confirmation in DivertBoxController: Is invalid model state or DivertCode: {confirmation.DivertCode}");
                var response = new
                {
                    divertCode = confirmation.DivertCode
                };
                return StatusCode(500, response);
            }

            try
            {
                var result = await _divertBoxAppService.DivertConfirm(timestampPLC, confirmation.TrackingId);
                if (result)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest(new { message = "Confirmation unsuccessful" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while confirming tote in Confirmation DivertBoxController.");
                return StatusCode(500, "An error occurred while confirm tote.");
            }
        }



    }
}
