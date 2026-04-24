using Luxottica.ApplicationServices.LimitSettings;
using Luxottica.ApplicationServices.Shared.Dto.Camera;
using Luxottica.ApplicationServices.Shared.Dto.LimitSettings;
using Luxottica.Controllers.Cameras;
using Luxottica.Controllers.JackpotLine;
using Luxottica.Core.Entities.LimitsSettings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Luxottica.Controllers.LimitSettings
{
    [Authorize]

    [Route("api/[controller]")]
    [ApiController]
    public class LimitSettingsController : ControllerBase
    {
        private readonly ILimitSettingsAppService _limitSettingsService;
        private readonly ILogger<LimitSettingsController> _logger;
        public LimitSettingsController(ILimitSettingsAppService limitSettingsService, ILogger<LimitSettingsController> logger)
        {
            _limitSettingsService = limitSettingsService;
            _logger = logger;
        }

        [HttpGet()]
        public async Task<IEnumerable<LimitSettingDTO>> GetAll()
        {
            try
            {
                List<LimitSettingDTO> limits = await _limitSettingsService.GetLimitSettingsAsync();
                return limits;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT LimitSettings IN LimitSettingsController Message {ex.Message}");
                throw new Exception("GetAll in LimitSettingsController unsuccessful");
            }

        }

        //[Authorize(Roles = "API.ReadOnly")]
        [HttpGet("{id}")]
        public async Task<LimitSettingDTO> GetById(int id)
        {
            try
            {
                LimitSettingDTO limit = await _limitSettingsService.GetLimitSettingByIdAsync(id);
                return limit;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT LimitSettings where Id = {id} IN LimitSettingsController Message {ex.Message}");
                throw new Exception("GetById in LimitSettingsController unsuccessful");
            }

        }

        [HttpGet("GetLimitInCam10")]
        public async Task<OkObjectResult> GetLimitsInCAM10()
        {
            try
            {
                LimitSettingDTO limit = await _limitSettingsService.GetLimitSettingInCam10();
                if (limit == null)
                {
                    return Ok(new { message = "Cam10 does not exist." });
                }
                else
                {
                    return Ok(limit);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR GetLimitsInCAM10 IN LimitSettingsController Message {ex.Message}");
                throw new Exception("GetLimitsInCAM10 in LimitSettingsController unsuccessful");
            }

        }

        //[Authorize(Roles = "API.ReadOnly")]
        [HttpPost]
        public async Task<IActionResult> Post(LimitSettingDTO entity)
        {
            try
            {
                if (entity.MaximumCapacity <= entity.CounterTote)
                {
                    _logger.LogError($"ERROR INSERT LimitSettings IN LimitSettingsController, Message: MaximumCapacity must be greater than CounterTote.");
                    return BadRequest("MaximumCapacity must be greater than CounterTote.");
                }

                var limit = await _limitSettingsService.AddLimitSettingsAsync(entity);
                return Ok(limit);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR INSERT LimitSettings IN LimitSettingsController Message {ex.Message}");
                return StatusCode(500, new { Message = $"{ex.Message}" });
            }
        }


        //[Authorize(Roles = "API.ReadOnly")]
        [HttpPut("{id}")]
        public async Task Put(int id, [FromBody] LimitSettingDTO model)
        {

            try
            {
                if (model.MaximumCapacity <= model.CounterTote)
                {
                    _logger.LogError($"ERROR Update LimitSettings where Id = {id} IN LimitSettingsController, Message: MaximumCapacity must be greater than CounterTote.");
                    BadRequest("MaximumCapacity must be greater than CounterTote.");
                }

                model.Id = id;
                await _limitSettingsService.EditLimitSettingAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR Update LimitSettings where Id = {id} IN LimitSettingsController Message {ex.Message}");
                throw new Exception($"{ex.Message}");
            }

        }

        [HttpPut("/limit/{id}")]
        public async Task<IActionResult> PutLimit(int id, int limitNumber)
        {
            try
            {
                LimitSettingDTO limit = await _limitSettingsService.GetLimitSettingByIdAsync(id);
                if (limit == null)
                {
                    _logger.LogError($"ERROR PutLimit where Id = {id} IN LimitSettingsController, Message:THE LIMIT WITH THAT ID DOES NOT EXIST {id}");
                    return BadRequest("THE LIMIT WITH THAT ID DOES NOT EXIST " + id);
                }
                var entity = new LimitSettingDTO
                {
                    Id = id,
                    CameraId = limit.CameraId,
                    MaximumCapacity = limitNumber,
                    CounterTote = limit.CounterTote,
                };
                await _limitSettingsService.EditLimitSettingAsync(entity);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR PutLimit with Id = {id} and limit number {limitNumber} in LimitSettingsController Message");
                return StatusCode(500, new { Message = $"{ex.Message}" });
            }

        }

        [HttpPut("/addtote/{id}")]
        public async Task<IActionResult> PutLimitNow(int id)
        {
            try
            {
                LimitSettingDTO limit = await _limitSettingsService.GetLimitSettingByIdAsync(id);
                if (limit == null)
                {
                    _logger.LogError($"ERROR PutLimitNow where Id = {id} IN LimitSettingsController, Message: THE LIMIT WITH THAT ID DOES NOT EXIST");
                    return BadRequest("THE LIMIT WITH THAT ID DOES NOT EXIST");
                }
                if (limit.CounterTote >= limit.MaximumCapacity)
                {
                    _logger.LogError($"ERROR PutLimitNow where Id = {id} IN LimitSettingsController, Message: No more totes can be added.");
                    return BadRequest("No more totes can be added.");
                }

                var entity = new LimitSettingDTO
                {
                    Id = id,
                    CameraId = limit.CameraId,
                    MaximumCapacity = limit.MaximumCapacity,
                    CounterTote = (limit.CounterTote ?? 0) + 1
                };
                await _limitSettingsService.EditLimitSettingAsync(entity);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR AddTote with Id = {id} in LimitSettingsController Message");
                return StatusCode(500, new { Message = $"{ex.Message}" });
            }

        }

        [HttpPut("/lesstote/{id}")]
        public async Task<IActionResult> PutLimitNowLess(int id)
        {
            try
            {
                LimitSettingDTO limit = await _limitSettingsService.GetLimitSettingByIdAsync(id);
                if (limit == null)
                {
                    _logger.LogError($"ERROR LessTote with Id = {id} in LimitSettingsController, Message: THE LIMIT WITH THAT ID DOES NOT EXIST");
                    return BadRequest("THE LIMIT WITH THAT ID DOES NOT EXIST");
                }
                if (limit.CounterTote <= 0)
                {
                    _logger.LogError($"ERROR LessTote with Id = {id} in LimitSettingsController, Message: Nothing to subtract");
                    return BadRequest("Nothing to subtract");
                }

                var entity = new LimitSettingDTO
                {
                    Id = id,
                    CameraId = limit.CameraId,
                    MaximumCapacity = limit.MaximumCapacity,
                    CounterTote = limit.CounterTote - 1,
                };
                await _limitSettingsService.EditLimitSettingAsync(entity);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR LessTote with Id = {id} in LimitSettingsController Message");
                return StatusCode(500, new { Message = $"{ex.Message}" });
            }

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _limitSettingsService.DeleteLimitSettingAsync(id);
                return Ok(new { message = "LimitSetting deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DELETE LimitSettings Where Id = {id} in LimitSettingsController Message");
                return StatusCode(500, new { message = $"{ex.Message}" });
            }
        }

    }
}
