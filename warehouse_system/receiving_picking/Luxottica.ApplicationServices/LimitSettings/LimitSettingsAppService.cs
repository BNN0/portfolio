using AutoMapper;
using Luxottica.ApplicationServices.Cameras;
using Luxottica.ApplicationServices.Shared.Dto.Camera;
using Luxottica.ApplicationServices.Shared.Dto.LimitSettings;
using Luxottica.Core.Entities.Cameras;
using Luxottica.Core.Entities.LimitsSettings;
using Luxottica.DataAccess.Repositories;
using Luxottica.DataAccess.Repositories.LimitSettings;
using Luxottica.DataAccess.Repositories.SecondLevelCameraAssignments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.LimitSettings
{
    public class LimitSettingsAppService : ILimitSettingsAppService
    {
        private readonly IRepository<int, LimitSetting> _repository;
        private readonly IMapper _mapper;
        private readonly LimitSettingRepository _LimitSettingRepository;
        private readonly ILogger<LimitSettingsAppService> _logger;

        public LimitSettingsAppService(IRepository<int, LimitSetting> repository, IMapper mapper,
            LimitSettingRepository limitSettingRepository, ILogger<LimitSettingsAppService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _LimitSettingRepository = limitSettingRepository;
            _logger = logger;
        }

        public async Task<int> AddLimitSettingsAsync(LimitSettingDTO limitSetting)
        {
            try
            {
                var entity = _mapper.Map<LimitSettingDTO, LimitSetting>(limitSetting);
                await _LimitSettingRepository.AddAsync(entity);
                return entity.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR INSERT LimitSettings IN AddLimitSettingsAsync SERVICE {ex.Message}");
                throw new Exception($"{ex.Message}");
            }
        }


        public async Task DeleteLimitSettingAsync(int limitSettingId)
        {
            try
            {
                await _repository.DeleteAsync(limitSettingId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DELETE LimitSettings IN DeleteLimitSettingAsync SERVICE {ex.Message}");
                throw new Exception($"{ex.Message}");
            }
        }

        public async Task EditLimitSettingAsync(LimitSettingDTO limitSetting)
        {
            try
            {
                var entity = _mapper.Map<LimitSettingDTO, LimitSetting>(limitSetting);
                await _LimitSettingRepository.UpdateAsync(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR UPDATE LimitSetting WHERE Id = {limitSetting.Id} IN EditLimitSettingAsync SERVICE {ex.Message}");
                throw new Exception($"{ex.Message}");
            }
        }

        public async Task<LimitSettingDTO> GetLimitSettingByIdAsync(int limitSettingId)
        {
            try
            {
                var entity = await _repository.GetAsync(limitSettingId);
                var dto = _mapper.Map<LimitSetting, LimitSettingDTO>(entity);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT LimitSetting WHERE Id = {limitSettingId} IN GetLimitSettingByIdAsync SERVICE {ex.Message}");
                throw new Exception($"{ex.Message}");
            }
        }


        public async Task<LimitSettingDTO> GetLimitSettingInCam10()
        {
            try
            {
                var entity = await _LimitSettingRepository.GetLimitInCam10();
                var dto = _mapper.Map<LimitSetting, LimitSettingDTO>(entity);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR GetLimitSettingInCam10 SERVICE {ex.Message}");
                throw new Exception($"{ex.Message}");
            }
        }


        public async Task<List<LimitSettingDTO>> GetLimitSettingsAsync()
        {
            try
            {
                var entities = await _repository.GetAll().ToListAsync();
                var dtos = _mapper.Map<List<LimitSetting>, List<LimitSettingDTO>>(entities);
                return dtos;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT LimitSettings IN GetLimitSettingsAsync SERVICE {ex.Message}");
                throw new Exception($"{ex.Message}");
            }
        }
    }
}
