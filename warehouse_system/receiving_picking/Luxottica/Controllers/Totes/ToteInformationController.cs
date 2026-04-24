using Luxottica.ApplicationServices.Cameras;
using Luxottica.ApplicationServices.DivertLines;
using Luxottica.ApplicationServices.JackpotLines;
using Luxottica.ApplicationServices.PhysicalMaps;
using Luxottica.ApplicationServices.Scanlogs;
using Luxottica.ApplicationServices.Shared.Dto.Scanlogs;
using Luxottica.ApplicationServices.Shared.Dto.ToteInformation;
using Luxottica.ApplicationServices.ToteInformations;
using Luxottica.Controllers.DivertOutboundLines;
using Luxottica.Core.Entities.ToteInformations;
using Luxottica.DataAccess.Repositories.Scanlogs;
using Luxottica.DataAccess.Repositories.ToteInformation;
using Luxottica.Models.NewTote;
using Luxottica.Models.Tote;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Text;
using static Luxottica.Controllers.Totes.Luxottica.Controllers.PhysicalMaps.ToteInformationController;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Luxottica.Controllers.Totes
{
    namespace Luxottica.Controllers.PhysicalMaps
    {
        [Authorize]
        [Route("api/[controller]")]
        [ApiController]
        public class ToteInformationController : ControllerBase
        {
            private readonly IToteInformationAppService _toteService;
            private readonly IJackpotLineAppService _jackpotLineAppService;
            private readonly IDivertLineService _divertLineService;
            private readonly IMapPhysicalAppService _mapPhysicalAppService;
            private readonly ILogger<ToteInformationController> _logger;
            private readonly HttpClient _httpClient;
            private readonly IScanlogsAppService _scanlogsAppService;
            public ToteInformationController(IToteInformationAppService toteService, HttpClient httpClient, IJackpotLineAppService jackpotLineAppService, IDivertLineService divertLineService, IMapPhysicalAppService mapPhysicalAppService, ILogger<ToteInformationController> logger, IScanlogsAppService scanlogsAppService)
            {
                _toteService = toteService;
                _httpClient = httpClient;
                _jackpotLineAppService = jackpotLineAppService;
                _divertLineService = divertLineService;
                _mapPhysicalAppService = mapPhysicalAppService;
                _logger = logger;
                _scanlogsAppService = scanlogsAppService;
            }

            //[Authorize(Role s = "API.ReadOnly")]
            [HttpGet()]
            public async Task<IEnumerable<ToteInformationE>> GetAll()
            {
                try
                {
                    List<ToteInformationE> totes = await _toteService.GetTotesAsync();
                    return totes;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"ERROR SELECT ToteInformation IN CONTROLLER, Message {ex.Message}");
                    throw new Exception("GetAll in ToteInformationController unsuccessful");
                }

            }

            //[Authorize(Roles = "API.ReadOnly")]
            [HttpGet("{id}")]
            public async Task<ToteInformationE> GetById(int id)
            {
                try
                {
                    ToteInformationE toteInformation = await _toteService.GetToteInformationByIdAsync(id);
                    return toteInformation;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"ERROR SELECT ToteInformation Where Id = {id} IN CONTROLLER, Message {ex.Message}");
                    throw new Exception("GetById in ToteInformationController unsuccessful");
                }

            }

            //[Authorize(Roles = "API.ReadOnly")]
            [HttpPost]
            public async Task<IActionResult> Post(ToteModel model)
            {
                try
                {
                    if (await VirtualToteExistsInDatabase(model.VirtualTote))
                    {
                        return BadRequest("The Virtual Tote already exists.");
                    }

                    var timestampPLC = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    var tote = new ToteInformationE
                    {
                        ToteLPN = model.toteLpn,
                        Timestamp = timestampPLC,
                        VirtualTote = model.VirtualTote,
                        ZoneDivertId = model.ZoneDivertId,
                        DivertStatus = null,
                        LineCount = model.LineCount,
                        TrackingId = model.TrackingId,
                    };

                    var toteInformation = await _toteService.AddToteInformationAsync(tote);
                    return Ok(toteInformation);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"ERROR INSERT ToteInformation IN CONTROLLER, Message {ex.Message}");
                    return StatusCode(500, "An error occurred while adding the Tote information.");
                }
            }


            //[Authorize(Roles = "API.ReadOnly")]
            [HttpPut("{id}")]
            public async Task<IActionResult> Put(int id, ToteModel model)
            {
                try
                {
                    var timestampPLC = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    var tote = new ToteInformationE
                    {
                        Id = id,
                        ToteLPN = model.toteLpn,
                        Timestamp = timestampPLC,
                        VirtualTote = model.VirtualTote,
                        ZoneDivertId = model.ZoneDivertId,
                        DivertStatus = model.DivertStatus,
                        LineCount = model.LineCount,
                        TrackingId = model.TrackingId,
                    };

                    await _toteService.EditToteInformationAsync(tote);
                    return Ok("Tote information updated successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"ERROR UPDATE ToteInformation Where Id = {id} IN CONTROLLER, Message {ex.Message}");
                    return StatusCode(500, "An error occurred while updating the Tote information.");
                }
            }

            //[Authorize(Roles = "API.ReadOnly")]
            [HttpDelete("{id}")]
            public async Task Delete(int id)
            {
                try
                {
                    await _toteService.DeleteToteInformationAsync(id);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"ERROR DELETE ToteInformation Where Id = {id} IN CONTROLLER, Message {ex.Message}");
                    throw new Exception("Delete in ToteInformationController unsuccessful");
                }

            }

            #region NewTote
            [HttpPost("receive-flow/new-tote")]
            public async Task<IActionResult> RegisterTote([FromBody] NewToteModel request)
            {
                try
                {
                    var sapInfo = new SapInfo();
                    var response = new Response();
                    if (request.toteLpn.StartsWith("T"))
                    {
                        sapInfo.Tote_LPN = request.toteLpn;
                        sapInfo.Virtual_Tote = "";
                        sapInfo.Zone_Id = 888;
                        sapInfo.Resp_Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");

                        var forCheckTote = await RegisterToteInDatabaseAsync(sapInfo, request.camId, request.trakingId);

                        if (forCheckTote != null && forCheckTote > 0)
                        {
                            await _toteService.CheckTote(sapInfo.Tote_LPN, request.trakingId);
                        }
                        return Ok(sapInfo);
                    }
                    else
                    {
                        response = await GetSapInfoAsync(request.toteLpn);
                    }


                    if (response != null && request.toteLpn != "?" && response.TOTE_ID.Length > 0 && response.V_TOTE_ID.Length > 0 && response.ZWZONE.Length > 0)
                    {
                        sapInfo.Tote_LPN = response.TOTE_ID;
                        sapInfo.Virtual_Tote = response.V_TOTE_ID;
                        sapInfo.Zone_Id = int.Parse(response.ZWZONE);
                        sapInfo.Resp_Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    }
                    else if (response != null && (response.TOTE_ID.Length <= 0 || response.V_TOTE_ID.Length <= 0 || response.ZWZONE.Length <= 0))
                    {
                        sapInfo.Tote_LPN = request.toteLpn;
                        sapInfo.Virtual_Tote = response.V_TOTE_ID;

                        var zoneVirtualGenerated = await _mapPhysicalAppService.GetJackpotAssignment();
                        sapInfo.Zone_Id = zoneVirtualGenerated;

                        sapInfo.Resp_Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    }
                    else
                    {
                        Console.WriteLine("Request to SAP unsuccessful");
                    }

                    if (sapInfo.Tote_LPN.Count() > 0)
                    {
                        // Register the new Tote in the database (simplified)
                        var forCheckTote = await RegisterToteInDatabaseAsync(sapInfo, request.camId, request.trakingId);

                        if (forCheckTote != null && forCheckTote > 0)
                        {
                            await _toteService.CheckTote(sapInfo.Tote_LPN, request.trakingId);
                        }

                        return Ok(sapInfo);
                    }

                    return BadRequest("Tote not registered");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"ERROR NEW TOTE METHOD IN CONTROLLER, Message {ex.Message}");
                    return StatusCode(500, "Internal server error: " + ex.Message);
                }
            }

            private async Task<int> RegisterToteInDatabaseAsync(SapInfo sapInfo, string Cam_Id, int trackingid)
            {
                try
                {
                    var newTote = new ToteInformationE
                    {
                        ToteLPN = sapInfo.Tote_LPN,
                        Timestamp = sapInfo.Resp_Timestamp, // Convert timestamp to a string if necessary
                        DivTimestamp = null,
                        VirtualTote = sapInfo.Virtual_Tote,
                        ZoneDivertId = sapInfo.Zone_Id,
                        DivertStatus = null,
                        LineCount = 0 // Make sure to assign the correct value if necessary
                    };

                    // Add the new Tote to the database context
                    var tote_sap = await _toteService.AddToteInformationAsync(newTote);

                    #region Scanlog
                    await _scanlogsAppService.AddScanlogAsync(new ScanlogsAddDto
                    {
                        ToteLPN = newTote.ToteLPN,
                        VirtualZone = newTote.ZoneDivertId,
                        VirtualTote = newTote.VirtualTote,
                        Wave = null,
                        TotesInWave = null,
                        TotalQty = null,
                        DestinationArea = null,
                        PutStation = null,
                        Status = null,
                        Release = null,
                        Processed = null,
                        StatusV10 = newTote.DivertStatus,
                        LapCount = newTote.LineCount,
                        TrackingId = newTote.TrackingId,
                        Timestamp = newTote.Timestamp,
                        CamId = Cam_Id,
                        DivertCode = 2,
                        Info = "Tote registered in V10 system"
                    });
                    #endregion

                    return tote_sap;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error registering the Tote in the database: " + ex.Message);
                    throw;
                }
            }


            private async Task<bool> VirtualToteExistsInDatabase(string tote)
            {
                try
                {
                    var totes = await _toteService.GetTotesAsync();

                    bool exists = totes.Any(t => t.VirtualTote == tote);
                    return exists;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error when performing tote existence validation.");
                }
            }

            public static async Task<Response> GetSapInfoAsync(string id)
            {
                using (HttpClient client = new HttpClient())
                {
                    string sapUrl = $"http://{ConnectionSAP.Ip}:{ConnectionSAP.Port}/api/sapinfo/send/{id}";
                    HttpResponseMessage response = await client.GetAsync(sapUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<Response>(responseContent);
                    }
                    return null;
                }
            }

            #endregion
            [HttpPost("receive-flow/borderPicking")]
            public async Task<IActionResult> ToteDesvio([FromBody] BorderLineModel borderLineModel)
            {
                try
                {
                    var divert_code = new DivertCodeBundleModel();

                    if (borderLineModel.scanner_N_lane_w_status == 1 && borderLineModel.scanner_N_lane_w_full == 0)
                    {
                        divert_code = await _toteService.ValidateTote(borderLineModel.ToteLPN, borderLineModel.CamId, borderLineModel.TrackingId);

                        return new OkObjectResult(divert_code);
                    }
                    else
                    {
                        divert_code.divert_code = 1;
                        divert_code.tracking_id = borderLineModel.TrackingId;
                        return new OkObjectResult(divert_code);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"ERROR BORDERPICKING METHOD IN CONTROLLER, Message {ex.Message}");
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
                }
            }

        }
    }
}
