using Luxottica.ApplicationServices.ToteInformations;
using Luxottica.Controllers.Totes.Luxottica.Controllers.PhysicalMaps;
using Luxottica.Models.NewTote;
using Luxottica.Models.TransferInboud;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Drawing;
using System.Reflection;
using System.Text;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Luxottica.Controllers.TransferInbound
{
    [Authorize]
    [Route("transfer-inboud")]
    [ApiController]
    public class TransferInboudController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IToteInformationAppService _toteService;
        private readonly ToteInformationController _toteInformationController;
        private readonly ILogger<TransferInboudController> _logger;
        public TransferInboudController(IToteInformationAppService toteService, IHttpClientFactory httpClient, ToteInformationController toteInformationController, ILogger<TransferInboudController> logger)
        {
            _toteService = toteService;
            _httpClient = httpClient.CreateClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(3);
            _toteInformationController = toteInformationController;
            _logger = logger;
        }

        [HttpPost("check-tote")]
        public async Task<IActionResult> CheckTote([FromBody] ToteScanInfoModel toteInfo)
        {
            var divert_code = new DivertCodeModel();

            //var zone_id = await getSAPToteInfo(toteInfo.LPN);
            var result = await _toteService.CheckTote(toteInfo.toteLpn, toteInfo.TrackingId/*, zone_id*/);

            if (result == 99)
            {
                try
                {
                    var newTote = new NewToteModel
                    {
                        camId = toteInfo.CamId,
                        trakingId = toteInfo.TrackingId,
                        toteLpn = toteInfo.toteLpn,
                    };

                    var apiInfo = await _toteInformationController.RegisterTote(newTote) as ObjectResult;

                    if (apiInfo.StatusCode == 200)
                    {
                        divert_code.divert_code = 2;
                        return new OkObjectResult(divert_code);
                    }
                    _logger.LogError($"ERROR CheckTote PickingJackpotLine where TOTELPN = {toteInfo.toteLpn} IN TransferInboudController, Message: Error in the tote registration process.");
                    throw new Exception("Error in the tote registration process.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"ERROR CheckTote PickingJackpotLine where TOTELPN = {toteInfo.toteLpn} IN TransferInboudController, Message: {ex.Message}.");
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $" {ex.Message}" });
                }
            }

            divert_code.divert_code = result;
            return new OkObjectResult(divert_code);
        }


        private async Task<int> getSAPToteInfo(string LPN)
        {
            string sapUrl = $"http://{ConnectionSAP.Ip}:{ConnectionSAP.Port}/api/sapinfo/send/{LPN}";
            var client = _httpClient;

            try
            {
                var response = await client.GetAsync(sapUrl);

                if (response.IsSuccessStatusCode == true)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var zwzone = JsonConvert.DeserializeObject<SapInfoResponse>(content);
                    return zwzone.ZWZONE;
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in the HTTP request: {ex.Message}");
            }
        }


    }
}
