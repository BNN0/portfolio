using LuxotticaSorting.ApplicationServices.RecirculationLimits;
using LuxotticaSorting.ApplicationServices.Shared.DTO.RecirculationLimits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LuxotticaSorting.Controllers.RecirculationLimits
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RecirculationLimitController : ControllerBase
    {
        private readonly IRecirculationLimitAppService _recirculationLimitService;
        private readonly ILogger<RecirculationLimitController> _logger;
        public RecirculationLimitController(IRecirculationLimitAppService recirculationLimitService, ILogger<RecirculationLimitController> logger)
        {
            _recirculationLimitService = recirculationLimitService;
            _logger = logger;
        }

        [Authorize(Roles = "SuperUser,Admin,ShippingAssociate")]
        [HttpGet()]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                List<RecirculationLimitDto> recirculationLimits = await _recirculationLimitService.GetRecirculationLimitValue();
                return Ok(recirculationLimits);
            }
            catch (Exception ex)
            {
                _logger.LogError($"In RecirculationLimitController method GetAll failed. error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }

        [Authorize(Roles = "SuperUser,Admin,ShippingAssociate")]
        [HttpPut("updateValue")]
        public async Task<IActionResult> ChangeValueRecirculationLimit(RecirculationLimitAddDto recirculationLimitAddDto)
        {
            try
            {
                await _recirculationLimitService.EditRecirculationLimitAddDto(recirculationLimitAddDto);
                return Ok(new { Message = "Successfully updated RecirculationLimits" });
            }
            catch(Exception ex)
            {
                _logger.LogError($"In RecirculationLimitController method ChangeValueRecirculationLimit failed: error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"{ex.Message}" });
            }
        }
    }
}
