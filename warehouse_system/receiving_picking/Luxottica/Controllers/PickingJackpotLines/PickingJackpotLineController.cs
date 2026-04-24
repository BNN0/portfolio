using Luxottica.ApplicationServices.JackpotLines;
using Luxottica.ApplicationServices.PickingJackpotLines;
using Luxottica.ApplicationServices.Shared.Dto.PickingJackpotLines;
using Luxottica.Controllers.Totes.Luxottica.Controllers.PhysicalMaps;
using Luxottica.Core.Entities.JackpotLines;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Luxottica.Controllers.PickingJackpotLines
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PickingJackpotLineController : ControllerBase
    {
        private readonly IPickingJackpotLineAppService _appService;
        private readonly ILogger<PickingJackpotLineController> _logger;
        public PickingJackpotLineController(IPickingJackpotLineAppService appService, ILogger<PickingJackpotLineController> logger)
        {
            _appService = appService;
            _logger = logger;
        }

        // GET: api/<PickingJackpotLineController>
        [HttpGet]
        public async Task<IEnumerable<PickingJackpotLineGetDto>> GetAll()
        {
            try
            {
                var pickingJackpots = await _appService.GetPickingJackpotLinesAsync();
                return pickingJackpots;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT PickingJackpotLine IN PickingJackpotLineController, Message {ex.Message}");
                throw new Exception("GetAll in PickingJackpotLineController unsuccessful");
            }
        }

        // DELETE api/<PickingJackpotLineController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _appService.DeletePickingJackpotLineAsync(id);

                if (response == null)
                {
                    _logger.LogError($"ERROR DELETE PickingJackpotLine IN PickingJackpotLineController, Message: The Picking Jackpot Line with that id does not exist");
                    return BadRequest(new { message = "The Picking Jackpot Line with that id does not exist" });
                }
                else
                {
                    return Ok(new { message = "The Picking Jackpot Line has been deleted" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DELETE PickingJackpotLine IN PickingJackpotLineController, Message {ex.Message}");
                return BadRequest(new { message = "An error occurred while deleting the Picking Jackpot Line" });
            }
        }


        [HttpPut("ChangeJackpot/{id}")]
        public async Task<IActionResult> ChangePickingJackpot(int id)
        {
            try
            {
                var result = await _appService.ChangeDivertLine(id);
                if (result == false)
                {
                    _logger.LogError($"ERROR UPDATE PickingJackpotLine where Id = {id} IN PickingJackpotLineController, Message: This DivertLine does not exist.");
                    return BadRequest(new { message = "This DivertLine does not exist." });
                };

                if (result == true)
                {
                    return Ok(result);
                }
                else
                {
                    _logger.LogError($"ERROR UPDATE PickingJackpotLine where Id = {id} IN PickingJackpotLineController, Message: The picking jackpot cannot be changed to a disabled divert line.");
                    return BadRequest($"{result}: The picking jackpot cannot be changed to a disabled divert line.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR UPDATE PickingJackpotLine where Id = {id} IN PickingJackpotLineController, Message {ex.Message}");
                throw new Exception("ChangePickingJackpot in PickingJackpotLineController unsuccessful");
            }
        }
    }
}
