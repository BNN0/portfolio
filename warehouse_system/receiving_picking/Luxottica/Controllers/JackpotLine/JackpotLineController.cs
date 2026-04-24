using Luxottica.ApplicationServices.JackpotLines;
using Luxottica.ApplicationServices.PhysicalMaps;
using Luxottica.ApplicationServices.Shared.Dto.JackpotLines;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Luxottica.Controllers.JackpotLine
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class JackpotLineController : ControllerBase
    {
        private readonly IJackpotLineAppService _jackpotLineAppService;
        private readonly ILogger<JackpotLineController> _logger;
        public JackpotLineController(IJackpotLineAppService jackpotLineAppService, ILogger<JackpotLineController> logger)
        {
            _jackpotLineAppService = jackpotLineAppService;
            _logger = logger;
        }

        [HttpGet()]
        public async Task<IEnumerable<JackpotLineDto>> GetAll()
        {
            try
            {
                List<JackpotLineDto> jackpots = await _jackpotLineAppService.GetJackpotLinesAsync();
                return jackpots;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT JackpotLine IN JackpotLineController Message {ex.Message}");
                throw new Exception("GetAll in JackpotLineController unsuccessful");
            }
        }


        [HttpGet("{id}")]
        public async Task<JackpotLineDto> GetById(int id)
        {
            try
            {
                JackpotLineDto jackpot = await _jackpotLineAppService.GetJackpotLineByIdAsync(id);
                return jackpot;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT JackpotLine Where Id = {id} IN JackpotLineController Message {ex.Message}");
                throw new Exception("GetById in JackpotLineController unsuccessful");
            }
        }

        [HttpPost("PostUpdate")]
        public async Task<IActionResult> PostUpdate(JackpotLineAddDto entity)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _jackpotLineAppService.UpdateJackpotState(entity);
                    return Ok();
                }
                _logger.LogError($"ERROR POST UPDATE IN JackpotLineController, Message: Bad request: The JSON is INVALID!");
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR POST UPDATE JackpotLine IN JackpotLineController Message {ex.Message}");
                throw new Exception("PostUpdate in JackpotLineController unsuccessful");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, JackpotLineAddDto model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _jackpotLineAppService.EditJackpotLinesAsync(id, model);
                    return Ok();
                }
                _logger.LogError($"ERROR UPDATE JackpotLine where Id = {id} IN JackpotLineController, Message: Bad request: The JSON is INVALID!");
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR UPDATE JackpotLine where Id = {id} IN JackpotLineController Message {ex.Message}");
                throw new Exception("Put in JackpotLineController unsuccessful");
            }
        }

        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            try
            {
                await _jackpotLineAppService.DeleteJackpotLinesAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DELETE JackpotLine where Id = {id} IN JackpotLineController Message {ex.Message}");
                throw new Exception("Delete in JackpotLineController unsuccessful");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(JackpotLineAddDto entity)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _jackpotLineAppService.AddJackpotLinesAsync(entity);
                    return Ok();
                }
                _logger.LogError($"ERROR INSERT JackpotLine where IN ChangeJackpot JackpotLineController, Message: Bad request: The JSON is INVALID!");
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR INSERT JackpotLine IN JackpotLineController Message {ex.Message}");
                throw new Exception("Post in JackpotLineController unsuccessful");
            }
        }

        [HttpPut("ChangeJackpot/{id}")]
        public async Task<IActionResult> ChangeJackpot(int id)
        {
            try
            {

                    var result = await _jackpotLineAppService.ChangeDivertline(id);
                    if (!result)
                    {
                    _logger.LogError($"ERROR Update JackpotLine where Id = {id} IN ChangeJackpot JackpotLineController, Message: This DivertLine does not exist or is turned off");
                    return NotFound(new { Message = $"This DivertLine does not exist or is turned off" });
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR Update JackpotLine where Id = {id} IN ChangeJackpot JackpotLineController Message: {ex.Message}");
                throw new Exception("ChangeJackpot in JackpotLineController unsuccessful");
            }
        }
    }
}
