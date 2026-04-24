using AutoMapper;
using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.Mapping.CarrierCodeDivertLaneMapping;
using LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodeDivertLaneMapping;
using LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodeLogisticAgent;
using LuxotticaSorting.Core.Mapping.CarrierCodeDivertLine;
using LuxotticaSorting.Core.Mapping.CarrierCodeLogisticAgent;
using LuxotticaSorting.DataAccess.Repositories.Mapping.CarrierCodeLogisticAgent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Mapping.CarrierCodeLogisticAgent
{
    public class CarrierCodeLogisticAgentAppService : ICarrierCodeLogisticAgentAppService
    {
        private readonly IRepository<int, CarrierCodeLogisticAgentMapping> _repository;
        private readonly CarrierCodeLogisticAgentRepository _carrierCodeLogisticAgentRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ICarrierCodeLogisticAgentAppService> _logger;
        public CarrierCodeLogisticAgentAppService(IRepository<int, CarrierCodeLogisticAgentMapping> repository, IMapper mapper, ILogger<ICarrierCodeLogisticAgentAppService> logger, CarrierCodeLogisticAgentRepository carrierCodeLogisticAgentRepository)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _carrierCodeLogisticAgentRepository = carrierCodeLogisticAgentRepository;
        }
        public async Task AddCarrierCodeLogisticAgentAsync(CarrierCodeLogisticAgentAddDto CarrierCodeLogisticAgent)
        {
            try
            {
                var c = _mapper.Map<CarrierCodeLogisticAgentMapping>(CarrierCodeLogisticAgent);
                await _repository.AddAsync(c);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CarrierCodeLogisticAgentAppService method addCarrierCodeLogisticAgentAsync failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteCarrierCodeLogisticAgentAsync(int CarrierCodeLogisticAgentId)
        {
            try
            {
                await _repository.DeleteAsync(CarrierCodeLogisticAgentId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CarrierCodeLogisticAgentAppService method DeleteCarrierCodeLogisticAgentAsync failed for {CarrierCodeLogisticAgentId}, error: {ex.Message}");
                throw;
            }
        }

        public async Task EditCarrierCodeLogisticAgentAsync(int id, CarrierCodeLogisticAgentAddDto CarrierCodeLogisticAgent)
        {
            try
            {
                var c = _mapper.Map<CarrierCodeLogisticAgentMapping>(CarrierCodeLogisticAgent);
                c.Id = id;
                await _repository.UpdateAsync(c);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CarrierCodeLogisticAgentAppService method EditCarrierCodeLogisticAgentAsync failed for {id}, error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<CarrierCodeLogisticAgentDto>> GetCarrierCodeLogisticAgentAsync()
        {
            try
            {
                var c = await _repository.GetAll().ToListAsync();
                List<CarrierCodeLogisticAgentDto> CarrierCodeLogisticAgent = _mapper.Map<List<CarrierCodeLogisticAgentDto>>(c);
                return CarrierCodeLogisticAgent;
            }
            catch (Exception ex)
            {
                _logger.LogError($"CarrierCodeLogisticAgentAppService method GetCarrierCodeLogisticAgentAsync failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task<CarrierCodeLogisticAgentDto> GetCarrierCodeLogisticAgentByIdAsync(int CarrierCodeLogisticAgentId)
        {
            try
            {
                var c = await _repository.GetAsync(CarrierCodeLogisticAgentId);
                CarrierCodeLogisticAgentDto dto = _mapper.Map<CarrierCodeLogisticAgentDto>(c);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"CarrierCodeLogisticAgentAppService method GetCarrierCodeLogisticAgentByIdAsync failed for {CarrierCodeLogisticAgentId}, error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<CarrierCodeLogisticAgentGetAllDto>> GetCombinedDataAsync()
        {
            try
            {
                var combinedDataList = await _carrierCodeLogisticAgentRepository.GetCombinedDataAsync();
                return combinedDataList;
            }
            catch (Exception ex)
            {
                _logger.LogError($"CarrierCodeLogisticAgentAppService method GetCombinedDataAsync failed, error: {ex.Message}");
                throw;
            }
        }

    }
}
