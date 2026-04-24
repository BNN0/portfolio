using Luxottica.ApplicationServices.Acknowledgments;
using Luxottica.ApplicationServices.CommissionerPackingLimits;
using Luxottica.ApplicationServices.Shared.Dto.CommisionerPackingLimits;
using Luxottica.Controllers.Commissioners;
using Luxottica.Core.Entities.Acknowledgments;
using Luxottica.Core.Entities.EXT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Luxottica.Controllers.CommissionerPackingLimits
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CommissionerPackingLimitsController : ControllerBase
    {
        private readonly ICommissionerPackingLimitAppService _commissionerPackingService;
        private readonly ILogger<CommissionerPackingLimitsController> _logger;
        public CommissionerPackingLimitsController(ICommissionerPackingLimitAppService commissionerPackingService, ILogger<CommissionerPackingLimitsController> logger)
        {
            _commissionerPackingService = commissionerPackingService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var commissionerPackingLimits = await _commissionerPackingService.GetLimitsPacking();
                return Ok(commissionerPackingLimits);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT Commisioner_Packing_Limits in CommissionerPackingLimitsController, Message: {ex.Message}.");
                return StatusCode(500, new { Message = $"Error while retrieving the limits of the machines with the commissioner present, Message: {ex.Message}" });
            }
        }



        [HttpPut]
        public async Task<IActionResult> UpdateLimits(CommisionerPackingLimitsRequest commisionerPackingLimitsRequest)
        {
            try
            {
                if (!SonTodosNumericos(commisionerPackingLimitsRequest))
                {
                    _logger.LogError("ERROR UpdateLimits in CommissionerPackingLimitsController, Message: Some values are not numeric.");
                    return BadRequest(new { Message = "Some values are not numeric." });
                }
                if (commisionerPackingLimitsRequest.SetLimitSuresort_1 < 1 || commisionerPackingLimitsRequest.SetLimitSuresort_2 < 1 || commisionerPackingLimitsRequest.SetLimitPutWall_1 < 1 || commisionerPackingLimitsRequest.SetLimitPutWall_2 < 1)
                {
                    _logger.LogError($"ERROR UpdateLimits in CommissionerPackingLimitsController, Message: The limits must be greater than or equal to one..");
                    return BadRequest(new { Message = "The limits must be greater than or equal to one." });
                }
                if (commisionerPackingLimitsRequest.SetLimitSuresort_1 > 100000 ||
    commisionerPackingLimitsRequest.SetLimitSuresort_2 > 100000 ||
    commisionerPackingLimitsRequest.SetLimitPutWall_1 > 100000 ||
    commisionerPackingLimitsRequest.SetLimitPutWall_2 > 100000)
                {
                    _logger.LogError("ERROR UpdateLimits in CommissionerPackingLimitsController, Message: The limits must be less than or equal to 10000.");
                    return BadRequest(new { Message = "The limits must be less than or equal to 100000." });
                }
                await _commissionerPackingService.UpdateCommissionerLimits(commisionerPackingLimitsRequest);
                return Ok(new { Message = "The limits of the machines with the commisioner present have been updated." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR UpdateLimits in CommissionerPackingLimitsController, Message: {ex.Message}.");
                return StatusCode(500, new { Message = $"Error while updating the limits of the machines with the commissioner present, Message: {ex.Message}" });
            }
        }
        private bool SonTodosNumericos(CommisionerPackingLimitsRequest request)
        {
            return EsNumero(request.SetLimitSuresort_1) &&
                   EsNumero(request.SetLimitSuresort_2) &&
                   EsNumero(request.SetLimitPutWall_1) &&
                   EsNumero(request.SetLimitPutWall_2);
        }

        private bool EsNumero(object valor)
        {
            return valor is int || valor is double || valor is float;
        }


    }
}
