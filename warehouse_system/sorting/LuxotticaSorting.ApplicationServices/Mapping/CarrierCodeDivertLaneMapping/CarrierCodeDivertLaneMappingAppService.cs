using AutoMapper;
using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodeDivertLaneMapping;
using LuxotticaSorting.DataAccess.Repositories.Mapping.CarrierCodeDivertLaneMappings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LuxotticaSorting.ApplicationServices.Mapping.CarrierCodeDivertLaneMapping
{
    public class CarrierCodeDivertLaneMappingAppService : ICarrierCodeDivertLaneMappingAppService
    {
        private readonly IRepository<int, Core.Mapping.CarrierCodeDivertLine.CarrierCodeDivertLaneMapping> _repository;
        private readonly CarrierCodeDivertLaneMappingRepository _repositoryCarrierCodeMapping;
        private readonly IMapper _mapper;
        private readonly ILogger<ICarrierCodeDivertLaneMappingAppService> _logger;
        public CarrierCodeDivertLaneMappingAppService(IRepository<int, Core.Mapping.CarrierCodeDivertLine.CarrierCodeDivertLaneMapping> repository, IMapper mapper, ILogger<ICarrierCodeDivertLaneMappingAppService> logger, CarrierCodeDivertLaneMappingRepository repositoryCarrierCodeMapping)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _repositoryCarrierCodeMapping = repositoryCarrierCodeMapping;
        }

        public async Task AddCarrierCodeDivertLaneMappingAsync(CarrierCodeDivertLaneMappingAddDto CarrierCodeDivertLaneMapping)
        {
            try
            {
                var c = _mapper.Map<Core.Mapping.CarrierCodeDivertLine.CarrierCodeDivertLaneMapping>(CarrierCodeDivertLaneMapping);
                await _repository.AddAsync(c);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CarrierCodeDivertLaneMappingAppService method AddCarrierCodeDivertLaneMappingAsync failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteCarrierCodeDivertLaneMappingAsync(int CarrierCodeDivertLaneMappingId)
        {
            try
            {
                await _repository.DeleteAsync(CarrierCodeDivertLaneMappingId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CarrierCodeDivertLaneMappingAppService method DeleteCarrierCodeDivertLaneMappingAsync failed for {CarrierCodeDivertLaneMappingId}, error: {ex.Message}");
                throw;
            }
        }

        public async Task EditCarrierCodeDivertLaneMappingAsync(int id, CarrierCodeDivertLaneMappingEditDto CarrierCodeDivertLaneMapping)
        {
            try
            {
                var c = _mapper.Map<Core.Mapping.CarrierCodeDivertLine.CarrierCodeDivertLaneMapping>(CarrierCodeDivertLaneMapping);
                c.Id = id;
                await _repository.UpdateAsync(c);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CarrierCodeDivertLaneMappingAppService method EditCarrierCodeDivertLaneMappingAsync failed for {id}, error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<CarrierCodeDivertLaneMappingDto>> GetCarrierCodeDivertLaneMappingAsync()
        {
            try
            {
                var c = await _repository.GetAll().ToListAsync();
                List<CarrierCodeDivertLaneMappingDto> CarrierCodeDivertLaneMapping = _mapper.Map<List<CarrierCodeDivertLaneMappingDto>>(c);
                return CarrierCodeDivertLaneMapping;
            }
            catch (Exception ex)
            {
                _logger.LogError($"CarrierCodeDivertLaneMappingAppService method GetCarrierCodeDivertLaneMappingAsync failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task<CarrierCodeDivertLaneMappingDto> GetCarrierCodeDivertLaneMappingByIdAsync(int CarrierCodeDivertLaneMappingId)
        {
            try
            {
                var c = await _repository.GetAsync(CarrierCodeDivertLaneMappingId);
                CarrierCodeDivertLaneMappingDto dto = _mapper.Map<CarrierCodeDivertLaneMappingDto>(c);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"CarrierCodeDivertLaneMappingAppService method GetCarrierCodeDivertLaneMappingByIdAsync failed for ID {CarrierCodeDivertLaneMappingId}, error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<CarrierCodeDivertLaneMappingGetAllDto>> GetCombinedDataAsync()
        {
            try
            {
                var combinedDataList = await _repositoryCarrierCodeMapping.GetCombinedDataAsync();
                return combinedDataList;
            }
            catch (Exception ex)
            {
                _logger.LogError($"CarrierCodeDivertLaneMappingAppService method GetCombinedDataAsync failed, error: {ex.Message}");
                throw;
            }
        }
    }
}
