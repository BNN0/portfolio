using AutoMapper;
using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.Mapping.CarrierCodeDivertLaneMapping;
using LuxotticaSorting.ApplicationServices.Shared.DTO.DivertLaneZebraConfigurationMapping;
using LuxotticaSorting.ApplicationServices.Shared.DTO.MappingSorters;
using LuxotticaSorting.Core.Mapping.DivertLaneZebraConfiguration;
using LuxotticaSorting.Core.MappingSorter;
using LuxotticaSorting.DataAccess.Repositories.DivertLaneZebraConfigurationMapping;
using LuxotticaSorting.DataAccess.Repositories.Mapping.CarrierCodeDivertLaneMappings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Mapping.DivertLaneZebraConfigurations
{
    public class DivertLaneZebraConfigurationAppService : IDivertLaneZebraConfigurationAppService
    {
        private readonly IRepository<int, DivertLaneZebraConfigurationMapping> _repository;
        private readonly DivertLaneZebraConfigurationMappingRepository _divertLaneZebraConfigurationMappingRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<DivertLaneZebraConfigurationAppService> _logger;
        public DivertLaneZebraConfigurationAppService(IRepository<int, DivertLaneZebraConfigurationMapping> repository,
            IMapper mapper, ILogger<DivertLaneZebraConfigurationAppService> logger,
            DivertLaneZebraConfigurationMappingRepository divertLaneZebraConfigurationMappingRepository)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _divertLaneZebraConfigurationMappingRepository = divertLaneZebraConfigurationMappingRepository;
        }

        public async Task AddDivertLaneZebraConfigurationMappingAsync(DivertLaneZebraConfigurationMappingAddDTO divertLaneZebraConfigurationMappingAddDTO)
        {
            try
            {
                var c = _mapper.Map<DivertLaneZebraConfigurationMapping>(divertLaneZebraConfigurationMappingAddDTO);
                await _repository.AddAsync(c);
            }
            catch (Exception ex)
            {
                _logger.LogError($"DivertLaneZebraConfigurationMappingRepository method AddDivertLaneZebraConfigurationMappingAsync failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteDivertLaneZebraConfigurationMappingAsync(int divertlaneZebraMappingId)
        {
            try
            {
                await _repository.DeleteAsync(divertlaneZebraMappingId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"DivertLaneZebraConfigurationMappingRepository method DeleteDivertLaneZebraConfigurationMappingAsync failed for {divertlaneZebraMappingId}, error: {ex.Message}");
                throw;
            }
        }

        public async Task EditDivertLaneZebraConfigurationMappingAsync(int id, DivertLaneZebraConfigurationMappingAddDTO divertLaneZebraConfigurationMappingAddDTO)
        {
            try
            {
                var c = _mapper.Map<DivertLaneZebraConfigurationMapping>(divertLaneZebraConfigurationMappingAddDTO);
                c.Id = id;
                await _divertLaneZebraConfigurationMappingRepository.UpdateAsync(c);
            }
            catch (Exception ex)
            {
                _logger.LogError($"DivertLaneZebraConfigurationMappingRepository method EditDivertLaneZebraConfigurationMappingAsync failed for {id}, error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<DivertLaneZebraConfigurationMappingDTO>> GetDivertLaneZebraConfigurationMappingAsync()
        {
            try
            {
                var c = await _repository.GetAll().ToListAsync();
                List<DivertLaneZebraConfigurationMappingDTO> MappingDivert = _mapper.Map<List<DivertLaneZebraConfigurationMappingDTO>>(c);
                return MappingDivert;
            }
            catch (Exception ex)
            {
                _logger.LogError($"DivertLaneZebraConfigurationMappingRepository method GetDivertLaneZebraConfigurationMappingAsync failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<DivertLanesZebraConfigurationCombinated>> GetDivertLaneZebraConfigurationMappingCombinatedDataAsync()
        {
            try
            {
                var c = await _divertLaneZebraConfigurationMappingRepository.GetCombinatedData();
                return c;
            }
            catch (Exception ex)
            {
                _logger.LogError($"DivertLaneZebraConfigurationMappingRepository method GetDivertLaneZebraConfigurationMappingCombinatedDataAsync failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task<DivertLaneZebraConfigurationMappingDTO> GetDivertLaneZebraConfigurationMappingByIdAsync(int divertlaneZebraMappingId)
        {
            try
            {
                var c = await _repository.GetAsync(divertlaneZebraMappingId);
                DivertLaneZebraConfigurationMappingDTO dto = _mapper.Map<DivertLaneZebraConfigurationMappingDTO>(c);

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"DivertLaneZebraConfigurationMappingRepository method GetDivertLaneZebraConfigurationMappingByIdAsync failed for {divertlaneZebraMappingId}, error: {ex.Message}");
                throw;
            }
        }
    }
}
