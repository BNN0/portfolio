using AutoMapper;
using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.PrintLabel;
using LuxotticaSorting.ApplicationServices.Shared.DTO.Zebra;
using LuxotticaSorting.ApplicationServices.Shared.DTO.ZebraHistorial;
using LuxotticaSorting.Core.Zebra;
using LuxotticaSorting.DataAccess.Repositories.PrintLabels;
using LuxotticaSorting.DataAccess.Repositories.ZebraHistorial;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.ZebraHistorial
{

    public class ZebraHistorialAppService : IZebraHistorialAppService
    {
        private readonly IRepository<int, Core.Zebra.ZebraHistorial> _repository;
        private readonly ILogger<ZebraHistorialAppService> _logger;
        private readonly ZebraHistorialRepository _zebraHistorialRepository;
        private readonly IMapper _mapper;
        public ZebraHistorialAppService(IRepository<int, Core.Zebra.ZebraHistorial> repository, ILogger<ZebraHistorialAppService> logger,
            ZebraHistorialRepository zebraHistorialRepository, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _zebraHistorialRepository = zebraHistorialRepository;
            _mapper = mapper;
        }

        public async Task DeleteZebraHistorialsAsync()
        {
            try
            {
                await _zebraHistorialRepository.DeleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"ZebraHistorialAppService method DeleteZebraHistorialsAsync failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task<ZebraHistorialDTO> GetZebraHistorialByIdAsync(int zebraHistorialId)
        {
            try
            {
                var c = await _repository.GetAsync(zebraHistorialId);
                ZebraHistorialDTO dto = _mapper.Map<ZebraHistorialDTO>(c);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ZebraHistorialAppService method GetZebraHistorialByIdAsync failed for {zebraHistorialId}, error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<ZebraHistorialDTO>> GetZebraHistorialsAsync()
        {
            try
            {
                var c = await _repository.GetAll().ToListAsync();
                List<ZebraHistorialDTO> confDTOs = _mapper.Map<List<ZebraHistorialDTO>>(c);
                return confDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ZebraHistorialAppService method GetZebraHistorialsAsync failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<ZebraHistorialData>> GetZebraHistorialsToRePrintAsync()
        {
            try
            {
                var c = await _zebraHistorialRepository.GetReprintGaylordAsync();
                return c;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ZebraHistorialAppService method GetZebraHistorialsToRePrintAsync failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<ZebraHistorialData>> GetZebraHistorialGaylordAsync()
        {
            try
            {
                var c = await _zebraHistorialRepository.GetGaylordAsync();
                return c;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ZebraHistorialAppService method GetZebraHistorialGaylordAsync failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<ZebraHistorialData>> GetZebraHistorialTruckAsync()
        {
            try
            {
                var c = await _zebraHistorialRepository.GetTruckAsync();
                return c;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ZebraHistorialAppService method GetZebraHistorialTruckAsync failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<ZebraHistorialData>> GetZebraHistorialToReprintTruckAsync()
        {
            try
            {
                var c = await _zebraHistorialRepository.GetTruckToReprintAsync();
                return c;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ZebraHistorialAppService method GetZebraHistorialToReprintTruckAsync failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<ZebraHistorialData>> GetZebraHistorialDataCombinatedAsync()
        {
            try
            {
                var c = await _zebraHistorialRepository.GetCombinatedDataAsync();
                return c;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ZebraHistorialAppService method GetZebraHistorialDataCombinatedAsync failed, error: {ex.Message}");
                throw;
            }
        }
    }
}
