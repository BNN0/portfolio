using AutoMapper;
using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.Mapping.CarrierCodeDivertLaneMapping;
using LuxotticaSorting.ApplicationServices.Shared.Dto.MappingSorters;
using LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodeDivertLaneMapping;
using LuxotticaSorting.ApplicationServices.Shared.DTO.MappingSorters;
using LuxotticaSorting.Core.Mapping.CarrierCodeDivertLine;
using LuxotticaSorting.DataAccess.Repositories.Mapping.CarrierCodeDivertLaneMappings;
using LuxotticaSorting.DataAccess.Repositories.MappingSorters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.MappingSorter
{
    public class MappingSorterAppService : IMappingSorterAppService
    {
        private readonly IRepository<int, Core.MappingSorter.MappingSorter> _repository;
        private readonly MappingSorterRepository _repositoryMappingSorter;
        private readonly IMapper _mapper;
        private readonly ILogger<IMappingSorterAppService> _logger;
        public MappingSorterAppService(IRepository<int, Core.MappingSorter.MappingSorter> repository, IMapper mapper, ILogger<IMappingSorterAppService> logger, MappingSorterRepository repositoryMappingSorter)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _repositoryMappingSorter = repositoryMappingSorter;
        }

        public async Task AddMappingSorterAsync()
        {
            try
            {
                var newMappingSorter = new Core.MappingSorter.MappingSorter();
                await _repository.AddAsync(newMappingSorter);
            }
            catch (Exception ex)
            {
                _logger.LogError($"MappingSorterAppService method AddMappingSorterAsync failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task EditMappingSorterAsync(int id, MappingSorterAddDto MappingSorter)
        {
            try
            {
                var c = _mapper.Map<Core.MappingSorter.MappingSorter>(MappingSorter);
                c.Id = id;
                await _repository.UpdateAsync(c);
            }
            catch (Exception ex)
            {
                _logger.LogError($"MappingSorterAppService method EditMappingSorterAsync failed for {id}, error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<MappingSorterDto>> GetMappingSorterAsync()
        {
            try
            {
                var c = await _repository.GetAll().ToListAsync();
                List<MappingSorterDto> MappingSorter = _mapper.Map<List<MappingSorterDto>>(c);
                return MappingSorter;
            }
            catch (Exception ex)
            {
                _logger.LogError($"MappingSorterAppService method GetMappingSorterAsync failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task<MappingSorterDto> GetMappingSorterByIdAsync(int MappingSorterId)
        {
            try
            {
                var c = await _repository.GetAsync(MappingSorterId);
                MappingSorterDto dto = _mapper.Map<MappingSorterDto>(c);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"MappingSorterAppService method GetMappingSorterByIdAsync failed for {MappingSorterId}, error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<MappingSorterGetAllDto>> GetCombinedDataAsync()
        {
            try
            {
                var combinedDataList = await _repositoryMappingSorter.GetCombinedDataAsync();
                return combinedDataList;
            }
            catch (Exception ex)
            {
                _logger.LogError($"MappingSorterAppService method GetCombinedDataAsync failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task AddMappingDivertLaneAndContainerTypeTruck(MappingSorterAddDivertContainerTyDto mappingDivertLaneContainerTypeTruck)
        {
            try
            {
                await _repositoryMappingSorter.AddMappingDivertLaneAndContainerTypeT(mappingDivertLaneContainerTypeTruck);
            }
            catch (Exception ex)
            {
                _logger.LogError($"MappingSorterAppService method AddMappingDivertLaneAndContainerTypeTruck failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task AddMappingDivertLaneAndContainerTypeGaylord(MappingSorterAddDivertContainerTyDto mappingDivertLaneContainerTypeGaylord)
        {
            try
            {
                await _repositoryMappingSorter.AddMappingDivertLaneAndContainerTypeG(mappingDivertLaneContainerTypeGaylord);
            }
            catch (Exception ex)
            {
                _logger.LogError($"MappingSorterAppService method AddMappingDivertLaneAndContainerTypeGaylord failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteDataContainerTypeInMappingSorter(int mapId)
        {
            try
            {
                await _repositoryMappingSorter.DeleteValueColumnContainerType(mapId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"MappingSorterAppService method DeleteDataContainerTypeInMappingSorter failed for {mapId}, error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<MappingSorterFrontendViewDto>> GetOnlyDivertLaneAndConainerType()
        {
            try
            {
                var onlyDivertLaneAndContainerType = await _repositoryMappingSorter.GetOnlyDivertLaneContainerType();
                return onlyDivertLaneAndContainerType;
            }
            catch (Exception ex)
            {
                _logger.LogError($"MappingSorterAppService method GetOnlyDivertLaneAndConainerType failed, error: {ex.Message}");
                throw;
            }
        }
    }
}
