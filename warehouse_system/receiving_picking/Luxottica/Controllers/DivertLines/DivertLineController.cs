using Luxottica.ApplicationServices.DivertLines;
using Luxottica.ApplicationServices.Shared.Dto.DivertLines;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Luxottica.Controllers.DivertLines
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DivertLineController : ControllerBase
    {
        private readonly IDivertLineService _divertLineService;
        private readonly ILogger<DivertLineController> _logger;
        public DivertLineController(IDivertLineService divertLineService, ILogger<DivertLineController> logger)
        {
            _divertLineService = divertLineService;
            _logger = logger;
        }

        [HttpGet()]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                List<DivertLineDto> divertLine = await _divertLineService.GetDivertLineAsync();
                return Ok(divertLine);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT DIVERTLINE CONTROLLER {ex.Message}");
                return StatusCode(500, new { Message = $"{ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                DivertLineDto divertLine = await _divertLineService.GetDivertLineByIdAsync(id);
                if (divertLine == null)
                {
                    _logger.LogError($"ERROR SELECT DIVERTLINE WHERE Id = {id} IN CONTROLLER, Message: DivertLine with ID {id} not found.");
                    return NotFound($"DivertLine with ID {id} not found.");
                }
                return Ok(divertLine);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT DIVERTLINE WHERE Id = {id} IN CONTROLLER {ex.Message}");
                return StatusCode(500, new { Message = $"{ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(DivertLineAddDto entity)
        {
            try
            {
                if (entity == null || entity.DivertLineValue == 0)
                {
                    _logger.LogError($"ERROR INSERT DIVERTLINE IN CONTROLLER, Message: The entity or the value of DivertLineValue cannot be null or equal to zero.");
                    return BadRequest("The entity or the value of DivertLineValue cannot be null or equal to zero.");
                }
                await _divertLineService.AddDivertLineAsync(entity);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR INSERT DIVERTLINE IN CONTROLLER {ex.Message}");
                return StatusCode(500, new { Message = $"{ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] DivertLineAddDto model)
        {
            try
            {
                if (id <= 0 || model == null || model.DivertLineValue == 0)
                {
                    _logger.LogError($"ERROR UPDATE DIVERTLINE IN CONTROLLER, Message: Invalid ID or model.");
                    return BadRequest("Invalid ID or model.");
                }
                await _divertLineService.EditDivertLineAsync(id, model);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR UPDATE DIVERTLINE WHERE Id = {id} IN CONTROLLER MESSAGE {ex.Message}");
                return StatusCode(500, new { Message = $"{ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _divertLineService.DeleteDivertLineAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DELETE DIVERTLINE WHERE Id = {id} IN CONTROLLER MESSAGE {ex.Message}");
                return StatusCode(500, new { Message = $"{ex.Message}" });
            }
        }
    }
}
