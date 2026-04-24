using Luxottica.ApplicationServices.DivertLines;
using Luxottica.ApplicationServices.PhysicalMaps;
using Luxottica.ApplicationServices.Shared.Dto.DivertLines;
using Luxottica.ApplicationServices.Shared.Dto.MapPhysicVirtualSAP;
using Luxottica.Core.Entities.DivertLines;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;

namespace Luxottica.Controllers.PhysicalMaps
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MapPhysicalVirualSAPController : ControllerBase
    {
        private readonly IMapPhysicalAppService _mapPhysicalService;
        private readonly ILogger<MapPhysicalVirualSAPController> _logger;
        public MapPhysicalVirualSAPController(IMapPhysicalAppService mapPhysicalService, ILogger<MapPhysicalVirualSAPController> logger)
        {
            _mapPhysicalService = mapPhysicalService;
            _logger = logger;
        }

        [HttpGet()]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                List<MapPhysicVirtualSAPDto> mapPhysical = await _mapPhysicalService.GetMapPhysicalAsync();
                return Ok(mapPhysical);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT MapPhysicalVirtualSAP IN MapPhysicalVirualSAPController {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                MapPhysicVirtualSAPDto mapPhysical = await _mapPhysicalService.GetMapPhysicalByIdAsync(id);
                if (mapPhysical == null)
                {
                    _logger.LogError($"ERROR SELECT MapPhysicalVirtualSAP WHERE Id = {id} IN MapPhysicalVirualSAPController, Message: Map Physical with Id: {id} not found!");
                    return NotFound($"Map Physical with Id: {id} not found!");
                }
                return Ok(mapPhysical);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT MapPhysicalVirtualSAP WHERE Id = {id} IN MapPhysicalVirualSAPController {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(MapPhysicVirtualSAPAddDto entity)
        {
            try
            {
                if (entity == null || entity.DivertLineId == 0 || entity.VirtualSAPZoneId == 0)
                {
                    _logger.LogError($"ERROR INSERT MapPhysicalVirtualSAP IN MapPhysicalVirualSAPController, Message: Entity is null or attributes are null.");
                    return BadRequest("Entity is null or attributes are null.");
                }
                await _mapPhysicalService.AddMapPhysicalAsync(entity);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR INSERT MapPhysicalVirtualSAP IN MapPhysicalVirualSAPController {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, MapPhysicVirtualSAPAddDto model)
        {
            try
            {
                if (model == null || model.DivertLineId == 0 || model.VirtualSAPZoneId == 0)
                {
                    _logger.LogError($"ERROR UPDATE MapPhysicalVirtualSAP WHERE Id = {id} IN MapPhysicalVirualSAPController, Message: Entity is null or attributes are null.");
                    return BadRequest("Entity is null or attributes are null.");
                }
                await _mapPhysicalService.EditMapPhysicalAsync(id, model);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR UPDATE MapPhysicalVirtualSAP WHERE Id = {id} IN MapPhysicalVirualSAPController {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _mapPhysicalService.DeleteMapPhysicalAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DELETE MapPhysicalVirtualSAP WHERE Id = {id} IN MapPhysicalVirualSAPController {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("heartbeat")]
        public bool Heartbeat()
        {
            try
            {
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [HttpGet("mapListByFilterId")]
        public async Task<IActionResult> GetMapPhysicalByDivertLineId(int divertLineId)
        {
            try
            {
                List<MapPhysicalGetAllDto> mapPhysicalList = await _mapPhysicalService.GetlistFilterByIdDiverLine(divertLineId);
                if (mapPhysicalList == null)
                {
                    _logger.LogError($"ERROR GetMapPhysicalByDivertLineId WHERE Id = {divertLineId} IN MapPhysicalVirualSAPController, Message: Map Physical with Id: {divertLineId} not found!");
                    return NotFound($"Map Physical with Id: {divertLineId} not found!");
                }
                return Ok(mapPhysicalList);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR GetMapPhysicalByDivertLineId with DivertLineId = {divertLineId} IN MapPhysicalVirualSAPController {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("mapListGetAll")]
        public async Task<IActionResult> GetMapPhysicalGetAll()
        {
            try
            {
                List<MapPhysicalGetAllDto> mapPhysicalList = await _mapPhysicalService.GetMapPhysicalAllNew();
                if (mapPhysicalList == null)
                {
                    _logger.LogError($"ERROR GetMapPhysicalGetAll IN MapPhysicalVirualSAPController, Message: Map Physical not found data.");
                    return NotFound("Map Physical not found data.");
                }
                return Ok(mapPhysicalList);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR GetMapPhysicalGetAll IN MapPhysicalVirualSAPController {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("mapJackpot")]
        public IActionResult GetDIvJ()
        {
            try
            {
                var r = _mapPhysicalService.GetDivertJackpot();
                return Ok(value: r);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR GetMapJackpot IN MapPhysicalVirualSAPController {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPut("updateValueJKMaps")]
        public async Task<IActionResult> PostMultipleMapsFromValueDiverLineJackpotAssigned(int oldJackpotId)
        {
            try
            {
                var m = await _mapPhysicalService.UpdateValueDiverJKMaps(oldJackpotId);

                return Ok(m);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR PostMultipleMapsFromValueDiverLineJackpotAssigned IN MapPhysicalVirualSAPController {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}
