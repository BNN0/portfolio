using AutoMapper;
using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.LogisticAgents;
using LuxotticaSorting.ApplicationServices.Shared.Dto.LogisticAgents;
using LuxotticaSorting.ApplicationServices.Shared.DTO.DivertLanes;
using LuxotticaSorting.ApplicationServices.Shared.DTO.MultiBoxWaves;
using LuxotticaSorting.Core.LogisticAgents;
using LuxotticaSorting.Core.MultiBoxWaves;
using LuxotticaSorting.DataAccess.Repositories.MultiBoxWaves;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.MultiBoxWaves
{
    public class MultiBoxWaveAppService : IMultiBoxWaveAppService
    {
        private readonly IRepository<int, MultiBoxWave> _repository;
        private readonly MultiBoxWaveRepository _multiBoxWaveRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<MultiBoxWaveAppService> _logger;
        public MultiBoxWaveAppService(IRepository<int, MultiBoxWave> repository, IMapper mapper, ILogger<MultiBoxWaveAppService> logger, MultiBoxWaveRepository multiBoxWaveRepository)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _multiBoxWaveRepository = multiBoxWaveRepository;
        }
        public async Task<(int, bool)> AddMultiBoxWavesAsync(MultiBoxWavesAddDto multiBoxWaves)
        {
            try
            {
                var l = _mapper.Map<MultiBoxWave>(multiBoxWaves);
                var result = await _multiBoxWaveRepository.AddMultiBoxWaveAsync(l);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"MultiBoxWaveAppService method AddMultiBoxWavesAsync failed, error: {ex.Message}");
                throw;
            }
        }
        public async Task<(int, bool)> ConfirmMultiBoxWaveAsync(MultiBoxWavesAddDto multiBoxWaves)
        {
            try
            {
                var l = _mapper.Map<MultiBoxWave>(multiBoxWaves);
                var result = await _multiBoxWaveRepository.ConfirmMultiBoxWaveAsync(l);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"MultiBoxWaveAppService method ConfirmMultiBoxWaveAsync failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteMultiBoxWavesAsync(int multiBoxWavesId)
        {
            try
            {
                await _repository.DeleteAsync(multiBoxWavesId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"MultiBoxWaveAppService method DeleteMultiBoxWavesAsync failed for {multiBoxWavesId}, error: {ex.Message}");
                throw;
            }
        }

        public async Task<MultiBoxWavesGetDto> GetMultiBoxWavesAsync(string confirmationNumber)
        {
            try
            {
                var l = await _multiBoxWaveRepository.GetMultiBoxWaveAsync(confirmationNumber);
                MultiBoxWavesGetDto dto = _mapper.Map<MultiBoxWavesGetDto>(l);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"MultiBoxWaveAppService method GetMultiBoxWavesAsync failed for {confirmationNumber}, error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<MultiBoxWavesGetDto>> GetAllMultiBoxWavesAsync()
        {
            try
            {
                var c = await _repository.GetAll().ToListAsync();
                List<MultiBoxWavesGetDto> multiBoxWavesDto = _mapper.Map<List<MultiBoxWavesGetDto>>(c);
                return multiBoxWavesDto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"MultiBoxWaveAppService method GetAllMultiBoxWavesAsync failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<MultiBoxWavesGetAllDto>> GetAllBoxMultiBoxWaveAsync()
        {
            try
            {
                var c = await _multiBoxWaveRepository.GetAllBoxMultiBoxWaveAsync();
                List<MultiBoxWavesGetAllDto> multiBoxWavesDto = _mapper.Map<List<MultiBoxWavesGetAllDto>>(c);
                return multiBoxWavesDto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"MultiBoxWaveAppService method GetAllBoxMultiBoxWaveAsync failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ManualConfirmationMultiBoxWavesForTrucks(MultiBoxWaveConfirmationDto multiBoxWaveConfirmation)
        {
            try
            {
                var result = await _multiBoxWaveRepository.ManualConfirmationMultiboxInWaves(multiBoxWaveConfirmation.BoxId, multiBoxWaveConfirmation.ConfirmationNumber);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"MultiBoxWaveAppService method ManualConfirmationMultiBoxWavesForTrucks failed, error: {ex.Message}");
                throw;
            }
        }


        public async Task<int?> MaxCountQtyConfiguration(int MaxCountQty)
        {
            try
            {
                var result = await _multiBoxWaveRepository.MaxCountQtyConfiguration(MaxCountQty);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"MultiBoxWaveAppService method MaxCountQtyConfiguration failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task<int?> GetMaxCountQtyConfiguration()
        {
            try
            {
                var result = await _multiBoxWaveRepository.GetMaxCountQtyConfiguration();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"MultiBoxWaveAppService method GetMaxCountQtyConfiguration failed, error: {ex.Message}");
                throw;
            }
        }
    }
}
