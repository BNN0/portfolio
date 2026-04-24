using AutoMapper;
using Luxottica.ApplicationServices.LimitSettings;
using Luxottica.ApplicationServices.Shared.Dto.MapPhysicVirtualSAP;
using Luxottica.Core.Entities.PhysicalMaps;
using Luxottica.DataAccess.Repositories;
using Luxottica.DataAccess.Repositories.PhysicalMaps;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Luxottica.ApplicationServices.PhysicalMaps
{
    public class MapPhysicalAppService : IMapPhysicalAppService
    {
        private readonly IRepository<int, MapPhysicalVirtualSAP> _repository;
        private readonly IMapper _mapper;
        private readonly MapPhysicalVirtualSAPRepository _mapRepository;
        private readonly ILogger<MapPhysicalAppService> _logger;

        public MapPhysicalAppService(IRepository<int, MapPhysicalVirtualSAP> repository, IMapper mapper, MapPhysicalVirtualSAPRepository mapRepository, ILogger<MapPhysicalAppService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _mapRepository = mapRepository;
            _logger = logger;
        }

        public async Task AddMapPhysicalAsync(MapPhysicVirtualSAPAddDto mapPhysical)
        {
            try
            {
                var m = _mapper.Map<Core.Entities.PhysicalMaps.MapPhysicalVirtualSAP>(mapPhysical);
                await _repository.AddAsync(m);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR INSERT MapPhysicalVirtualSAP IN AddMapPhysicalAsync SERVICE {ex.Message}");
                throw new Exception("AddMapPhysicalAsync in MapPhysicalAppService unsuccessful");
            }
        }

        public async Task DeleteMapPhysicalAsync(int mapPhysicalId)
        {
            try
            {
                await _repository.DeleteAsync(mapPhysicalId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DELETE MapPhysicalVirtualSAP where Id = {mapPhysicalId} IN DeleteMapPhysicalAsync SERVICE {ex.Message}");
                throw new Exception($"DeleteMapPhysicalAsync in MapPhysicalAppService unsuccessful for ID: {mapPhysicalId}");
            }
        }

        public async Task EditMapPhysicalAsync(int id, MapPhysicVirtualSAPAddDto mapPhysical)
        {
            try
            {
                var m = _mapper.Map<MapPhysicalVirtualSAP>(mapPhysical);
                m.Id = id;
                await _repository.UpdateAsync(m);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR UPDATE MapPhysicalVirtualSAP Where Id = {id} IN EditMapPhysicalAsync SERVICE {ex.Message}");
                throw new Exception($"EditMapPhysicalAsync in MapPhysicalAppService unsuccessful for ID: {id}");
            }
        }

        public async Task<List<MapPhysicVirtualSAPDto>> GetMapPhysicalAsync()
        {
            try
            {
                var m = await _repository.GetAll().ToListAsync();
                List<MapPhysicVirtualSAPDto> mapPhysics = _mapper.Map<List<MapPhysicVirtualSAPDto>>(m);
                return mapPhysics;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR Select MapPhysicalVirtualSAP IN GetMapPhysicalAsync SERVICE {ex.Message}");
                throw new Exception("GetMapPhysicalAsync in MapPhysicalAppService unsuccessful");
            }
        }

        public async Task<MapPhysicVirtualSAPDto> GetMapPhysicalByIdAsync(int mapPhysicalId)
        {
            try
            {
                var m = await _repository.GetAsync(mapPhysicalId);
                MapPhysicVirtualSAPDto dto = _mapper.Map<MapPhysicVirtualSAPDto>(m);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR Select MapPhysicalVirtualSAP WHERE Id = {mapPhysicalId} IN GetMapPhysicalByIdAsync SERVICE {ex.Message}");
                throw new Exception($"GetMapPhysicalByIdAsync in MapPhysicalAppService unsuccessful for ID: {mapPhysicalId}");
            }
        }

        public int GetDivertJackpot()
        {
            try
            {
                var j = _mapRepository.GetDiverJack();
                return j;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR GetDivertJackpot Method SERVICE {ex.Message}");
                throw new Exception("GetDivertJackpot in MapPhysicalAppService unsuccessful");
            }
        }

        public async Task<int> UpdateValueDiverJKMaps(int oldJackpotId)
        {
            try
            {
                int m = await _mapRepository.UpdateValuesDiverlineJk(oldJackpotId);
                return m;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR UpdateValueDiverJKMaps with Id = {oldJackpotId} Method SERVICE {ex.Message}");
                throw new Exception($"UpdateValueDiverJKMaps in MapPhysicalAppService unsuccessful for oldJackpotId: {oldJackpotId}");
            }
        }

        public async Task<List<MapPhysicalGetAllDto>> GetlistFilterByIdDiverLine(int mapDiverlineId)
        {
            try
            {
                var m = await _mapRepository.GetByDivertLineId(mapDiverlineId).ToListAsync();
                List<MapPhysicalGetAllDto> mapPhysicsList = _mapper.Map<List<MapPhysicalGetAllDto>>(m);
                return mapPhysicsList;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR GetlistFilterByIdDiverLine with Id = {mapDiverlineId} Method SERVICE {ex.Message}");
                throw new Exception($"GetlistFilterByIdDiverLine in MapPhysicalAppService unsuccessful for mapDiverlineId: {mapDiverlineId}");
            }
        }

        public async Task<List<MapPhysicalGetAllDto>> GetMapPhysicalAllNew()
        {
            try
            {
                var m = await _mapRepository.GetAllMaps();
                return m;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR GetMapPhysicalAllNew Method SERVICE {ex.Message}");
                throw new Exception("GetMapPhysicalAllNew in MapPhysicalAppService unsuccessful");
            }
        }

        public async Task<int> GetJackpotAssignment()
        {
            var result = await _mapRepository.GetJackpotAssignment();

            return result;
        }
    }
}
