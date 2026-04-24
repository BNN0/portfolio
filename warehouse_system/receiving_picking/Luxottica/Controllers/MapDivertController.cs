using Luxottica.ApplicationServices.DivertLines;
using Luxottica.ApplicationServices.MapDivert;
using Luxottica.ApplicationServices.PhysicalMaps;
using Luxottica.ApplicationServices.Shared.Dto.DivertLines;
using Luxottica.ApplicationServices.Shared.Dto.MapDivert;
using Luxottica.ApplicationServices.Shared.Dto.MapPhysicVirtualSAP;
using Luxottica.Controllers.PhysicalMaps;
using Luxottica.Controllers.TransferInbound;
using Luxottica.Core.Entities.DivertLines;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Numerics;
using static Luxottica.Controllers.PhysicalMaps.MapPhysicalVirualSAPController;

namespace Luxottica.Controllers
{
    [Authorize]
    public class MapDivertController : Controller
    {
        private readonly IMapDivertAppService _mapDivertAppService;
        private readonly ILogger<MapDivertController> _logger;
        public MapDivertController(IMapDivertAppService mapDivertAppService, ILogger<MapDivertController> logger)
        {
            _mapDivertAppService = mapDivertAppService;
            _logger = logger;
        }
        [HttpPut("/map/{id}")]
        public async Task<IActionResult> GetList(int id, [FromBody] VectorModel vector)
        {
            
            try
            {
                if (id == null || id <= 0)
                {
                    _logger.LogError($"ERROR UPDATE MapDivertLine WHERE Id = {id} IN MapDivertController, Message: The id must be greater than 0");
                    throw new Exception("The id must be greater than 0");
                }
                if (vector == null || vector.Values == null || vector.Values.Count == 0)
                {
                    _logger.LogError($"ERROR UPDATE MapDivertLine WHERE Id = {id} IN MapDivertController, Message: The vector is empty or null");
                    throw new Exception("The vector is empty or null");
                }
                bool exceeds = vector.Values.Any(value => value > 99);
                if (exceeds == true)
                {
                    _logger.LogError($"ERROR UPDATE MapDivertLine WHERE Id = {id} IN MapDivertController, Message: Values must not be greater than 99.");
                    throw new Exception("Values must not be greater than 99.");
                }

                var assign = await _mapDivertAppService.AssignVirtualZones(id, vector);
                if (assign == false)
                {
                    _logger.LogError($"ERROR UPDATE MapDivertLine WHERE Id = {id} IN MapDivertController, Message: This DivertLine does not exist or some data are wrong.");
                    throw new Exception("This DivertLine does not exist or some data are wrong");
                }
                else
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR UPDATE MapDivertLine WHERE Id = {id} IN MapDivertController, Message: {ex.Message}");
                return StatusCode(500, new { Message = $"{ex.Message}" });
            }
        }
        [HttpPut("/mapDivert/{id}")]
        public async Task<IActionResult> DisAssign(int id)
        {
           
            try
            {
                if (id == null || id <= 0)
                {
                    _logger.LogError($"ERROR DisAssign WHERE DIVERTLINE = {id} IN MapDivertController, Message: The id must be greater than 0");
                    throw new Exception("The id must be greater than 0");
                }
                var assign = await _mapDivertAppService.DisAssignVirtualZones(id);
                if (assign == false)
                {
                    _logger.LogError($"ERROR DisAssign WHERE DIVERTLINE = {id} IN MapDivertController, Message: This DivertLine does not exist");
                    throw new Exception("This DivertLine does not exist");
                }
                else
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DisAssign WHERE DIVERTLINE = {id} IN MapDivertController, Message:{ex.Message}");
                return StatusCode(500, new { Message = $"{ex.Message}" });
            }
        }

    }
}
