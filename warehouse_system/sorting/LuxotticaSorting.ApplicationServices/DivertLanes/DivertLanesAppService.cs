using AutoMapper;
using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.BoxTypes;
using LuxotticaSorting.ApplicationServices.Shared.DTO.BoxTypes;
using LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodeDivertLaneMapping;
using LuxotticaSorting.ApplicationServices.Shared.DTO.DivertLanes;
using LuxotticaSorting.Core.BoxTypes;
using LuxotticaSorting.Core.DivertLanes;
using LuxotticaSorting.Core.Mapping.CarrierCodeDivertLine;
using LuxotticaSorting.DataAccess.Repositories.BoxTypes;
using LuxotticaSorting.DataAccess.Repositories.DivertLanes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.DivertLanes
{
    public class DivertLanesAppService : IDivertLanesAppService
    {
        private readonly IRepository<int, DivertLane> _repository;
        private readonly ILogger<DivertLanesAppService> _logger;
        private readonly DivertLanesRepository _divertLanesRepository;
        private readonly IMapper _mapper;
        public DivertLanesAppService(IRepository<int, DivertLane> repository, ILogger<DivertLanesAppService> logger,
             DivertLanesRepository divertLanesRepository,IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _divertLanesRepository = divertLanesRepository;
            _mapper = mapper;
        }
        public async Task<int> AddDivertLaneAsync(DivertLanesAddDto divertLanesAddDto)
        {
            try
            {
                var c = _mapper.Map<DivertLane>(divertLanesAddDto);
                var id = await _repository.AddAsync(c);
                return id.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError($"DivertLanesAppService method AddDivertLaneAsync failed, error: {ex.Message}");
                throw;
            }
        }


        public async Task<int> AddDivertLaneCreationAsync(DivertLanesAddCreationDTO divertLanesAddDto)
        {
            try
            {
                var id = await _divertLanesRepository.AddAsyncCreation(divertLanesAddDto);
                return id.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError($"DivertLanesAppService method AddDivertLaneAsync failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteDivertLaneAsync(int divertLanesId)
        {
            try
            {
                await _divertLanesRepository.DeleteAsync(divertLanesId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"DivertLanesAppService method DeleteDivertLaneAsync error occurred while deleting DivertLane {divertLanesId}, error: {ex.Message}");
                throw;
            }
        }

        public async Task EditDivertLaneAsync(int id, DivertLanesAddDto divertLanesAddDto)
        {
            try
            {
                var c = _mapper.Map<DivertLane>(divertLanesAddDto);
                c.Id = id;
                await _repository.UpdateAsync(c);
            }
            catch (Exception ex)
            {
                _logger.LogError($"DivertLanesAppService method EditDivertLaneAsync failed for {id}, error: {ex.Message}");
                throw;
            }
        }


        public async Task EditDivertLaneCreationAsync(int id, DivertLanesAddCreationDTO divertLanesAddDto)
        {
            try
            {
                await _divertLanesRepository.UpdateCreationAsync(id, divertLanesAddDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"DivertLanesAppService method EditDivertLaneAsync failed for {id}, error: {ex.Message}");
                throw;
            }
        }

        public async Task<DivertLanesDTO> GetDivertLaneByIdAsync(int divertLanesId)
        {
            try
            {
                var c = await _repository.GetAsync(divertLanesId);
                DivertLanesDTO dto = _mapper.Map<DivertLanesDTO>(c);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"DivertLanesAppService method GetDivertLaneByIdAsync failed for {divertLanesId}, error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<DivertLanesDTO>> GetDivertLanesAsync()
        {
            try
            {
                var c = await _repository.GetAll().ToListAsync();
                List<DivertLanesDTO> divertLanesDTOs = _mapper.Map<List<DivertLanesDTO>>(c);
                divertLanesDTOs = divertLanesDTOs.OrderBy(dto => dto.DivertLanes).ToList();
                return divertLanesDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError($"DivertLanesAppService method GetDivertLanesAsync failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<DivertLanesDTO>> GetDivertLanesToPrintTruckAsync()
        {
            try
            {
                var c = await _divertLanesRepository.GetDivertLanesTruckToPrint();
                List<DivertLanesDTO> divertLanesDTOs = _mapper.Map<List<DivertLanesDTO>>(c);
                divertLanesDTOs = divertLanesDTOs.OrderBy(dto => dto.DivertLanes).ToList();
                return divertLanesDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError($"DivertLanesAppService method GetDivertLanesToPrintTruckAsync failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<DivertLanesDTO>> GetDivertLanesToPrintGaylordAsync()
        {
            try
            {
                var c = await _divertLanesRepository.GetDivertLanesGaylordToPrint();
                List<DivertLanesDTO> divertLanesDTOs = _mapper.Map<List<DivertLanesDTO>>(c);
                divertLanesDTOs = divertLanesDTOs.OrderBy(dto => dto.DivertLanes).ToList();
                return divertLanesDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError($"DivertLanesAppService method GetDivertLanesToPrintGaylordAsync failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task<DivertLanesCreationDTO> GetDivertLaneCreationByIdAsync(int divertLanesId)
        {
            try
            {
                var c = await _repository.GetAsync(divertLanesId);
                DivertLanesCreationDTO dto = _mapper.Map<DivertLanesCreationDTO>(c);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"DivertLanesAppService method GetDivertLaneByIdAsync failed for {divertLanesId}, error: {ex.Message}");
                throw;
            }
        }


        public async Task<List<DivertLanesCreationDTO>> GetDivertLanesCreationAsync()
        {
            try
            {
                var c = await _repository.GetAll().ToListAsync();
                List<DivertLanesCreationDTO> divertLanesDTOs = _mapper.Map<List<DivertLanesCreationDTO>>(c);
                divertLanesDTOs = divertLanesDTOs.OrderBy(dto => dto.DivertLanes).ToList();
                return divertLanesDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError($"DivertLanesAppService method GetDivertLanesCreationAsync failed, error: {ex.Message}");
                throw;
            }
        }
    }
}
