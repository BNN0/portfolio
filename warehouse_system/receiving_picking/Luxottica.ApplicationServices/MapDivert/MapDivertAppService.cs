using AutoMapper;
using Luxottica.ApplicationServices.DivertLines;
using Luxottica.ApplicationServices.Shared.Dto.DivertLines;
using Luxottica.ApplicationServices.Shared.Dto.DivertOutboundLines;
using Luxottica.ApplicationServices.Shared.Dto.MapDivert;
using Luxottica.Core.Entities.DivertOutboundLines;
using Luxottica.DataAccess.Repositories;
using Luxottica.DataAccess.Repositories.DivertOutboundLines;
using Luxottica.DataAccess.Repositories.MapDivert;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.MapDivert
{
    public class MapDivertAppService : IMapDivertAppService
    {
        private readonly MapDivertRepository _MapDivertRepository;
        private readonly ILogger<MapDivertAppService> _logger;
        public MapDivertAppService(MapDivertRepository mapDivertRepository, ILogger<MapDivertAppService> logger)
        {
            _MapDivertRepository = mapDivertRepository;
            _logger = logger;
        }
        public async Task<bool> AssignVirtualZones(int id, VectorModel vector)
        {
            try
            {
                var t = await _MapDivertRepository.AssignVirtualZone(id, vector);
                if (t == false)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR AssignVirtualZones WHERE Id = {id} IN MapDivertAppService, Message: {ex.Message}");
                throw new Exception($"{ex.Message}");
            }
        }

        public async Task<bool> DisAssignVirtualZones(int id)
        {
            try
            {
                var t = await _MapDivertRepository.OnlyDivertValue(id);
                if (t == false)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DisAssignVirtualZones WHERE Id = {id} IN MapDivertAppService, Message: {ex.Message}");
                throw new Exception($"{ex.Message}");
            }
        }
    }
}
