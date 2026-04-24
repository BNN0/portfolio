using AutoMapper;
using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.Containers;
using LuxotticaSorting.ApplicationServices.Shared.DTO.Containers;
using LuxotticaSorting.ApplicationServices.Shared.DTO.PrintLabel;
using LuxotticaSorting.ApplicationServices.Shared.DTO.Zebra;
using LuxotticaSorting.Core.Container;
using LuxotticaSorting.Core.ContainerTypes;
using LuxotticaSorting.Core.Zebra;
using LuxotticaSorting.DataAccess.Repositories.Containers;
using LuxotticaSorting.DataAccess.Repositories.PrintLabels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.PrintLabel
{
    public class PrintLabelAppService : IPrintLabelAppService
    {

        private readonly IRepository<int, ZebraConfiguration> _repository;
        private readonly ILogger<PrintLabelAppService> _logger;
        private readonly PrintLabelRepository _printLabelRepository;
        private readonly IMapper _mapper;
        public PrintLabelAppService(IRepository<int, ZebraConfiguration> repository, ILogger<PrintLabelAppService> logger,
            PrintLabelRepository printLabelRepository, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _printLabelRepository = printLabelRepository;
            _mapper = mapper;
        }

        public async Task<int> AddZebraConfigurationAsync(ZebraConfigurationAddDTO zebraConfigurationAddDTO)
        {
            try
            {
                var c = _mapper.Map<ZebraConfiguration>(zebraConfigurationAddDTO);
                var id = await _printLabelRepository.AddAsync(c);
                return id.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError($"PrintLabelAppService method AddZebraConfigurationAsync failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteZebraConfigurationAsync(int zebraConfigurationId)
        {
            try
            {
                await _repository.DeleteAsync(zebraConfigurationId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"PrintLabelAppService method DeleteZebraConfigurationAsync failed for {zebraConfigurationId}, error: {ex.Message}");
                throw;
            }
        }

        public async Task EditZebraConfigurationAsync(int id, ZebraConfigurationAddDTO zebraConfigurationAddDTO)
        {
            try
            {
                var c = _mapper.Map<ZebraConfiguration>(zebraConfigurationAddDTO);
                c.Id = id;
                await _printLabelRepository.UpdateAsync(c);
            }
            catch (Exception ex)
            {
                _logger.LogError($"PrintLabelAppService method EditZebraConfigurationAsync failed for {id}, error: {ex.Message}");
                throw;
            }
        }

        public async Task<ZebraConfigurationDTO> GetZebraConfigurationByIdAsync(int zebraConfigurationId)
        {
            try
            {
                var c = await _repository.GetAsync(zebraConfigurationId);
                ZebraConfigurationDTO dto = _mapper.Map<ZebraConfigurationDTO>(c);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"PrintLabelAppService method GetZebraConfigurationByIdAsync failed for {zebraConfigurationId}, error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<ZebraConfigurationDTO>> GetZebraConfigurationsAsync()
        {
            try
            {
                var c = await _repository.GetAll().ToListAsync();
                List<ZebraConfigurationDTO> confDTOs = _mapper.Map<List<ZebraConfigurationDTO>>(c);
                return confDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError($"PrintLabelAppService method GetZebraConfigurationsAsync failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> PrintLabelService(PrintLabelDTO printLabelDTO)
        {
            try
            {
                var response = await _printLabelRepository.PrintLabel(printLabelDTO);
                if (response.Value == true)
                {
                    return true;     
                }
                else{
                    throw new Exception($"{response.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"PrintLabelAppService method PrintLabelService failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> PrintLabelReprint(PrintLabelReprint printLabelDTO)
        {
            try
            {
                var response = await _printLabelRepository.Reprint(printLabelDTO);
                if (response.Value == true)
                {
                    return true;

                }
                else
                {
                    throw new Exception($"{response.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"PrintLabelAppService method PrintLabelReprint failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> PrintLabelManualService(PrintManualDTO printLabelDTO)
        {
            try
            {
                var response = await _printLabelRepository.PrintLabelManual(printLabelDTO);
                if (response.Value == true)
                {
                    return true;
                }
                else
                {
                    throw new Exception($"{response.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"PrintLabelAppService method PrintLabelManualService failed, error: {ex.Message}");
                throw;
            }
        }
    }
}
