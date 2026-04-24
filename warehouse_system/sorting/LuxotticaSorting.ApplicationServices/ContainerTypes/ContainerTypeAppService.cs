using AutoMapper;
using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.Shared.Dto.ContainerTypes;
using LuxotticaSorting.Core.ContainerTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.ContainerTypes
{
    public class ContainerTypeAppService : IContainerTypeAppService
    {
        private readonly IRepository<int, ContainerType> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<ContainerTypeAppService> _logger;
        public ContainerTypeAppService(IRepository<int, ContainerType> repository, IMapper mapper, ILogger<ContainerTypeAppService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task AddContainerTypeAsync(ContainerTypeAddDto containerType)
        {
            try
            {
                var c = _mapper.Map<ContainerType>(containerType);
                await _repository.AddAsync(c);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ContainerTypeAppService method AddContainerTypeAsync failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteContainerTypeAsync(int containerTypeId)
        {
            try
            {

                await _repository.DeleteAsync(containerTypeId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ContainerTypeAppService method DeleteContainerTypeAsync failed for {containerTypeId}, error: {ex.Message}");
                throw;
            }
        }

        public async Task EditContainerTypeAsync(int id, ContainerTypeAddDto containerType)
        {
            try
            {
                var c = _mapper.Map<ContainerType>(containerType);//replace real entity DB
                c.Id = id;
                await _repository.UpdateAsync(c);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ContainerTypeAppService method EditContainerTypeAsync failed for {id}, error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<ContainerTypeDto>> GetContainerTypesAsync()
        {
            try
            {
                var c = await _repository.GetAll().ToListAsync();
                List<ContainerTypeDto> containerTypes = _mapper.Map<List<ContainerTypeDto>>(c);
                return containerTypes;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ContainerTypeAppService method ContainerTypeDto failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task<ContainerTypeDto> GetContainerTypeByIdAsync(int containerTypeId)
        {
            try
            {
                var c = await _repository.GetAsync(containerTypeId);
                ContainerTypeDto dto = _mapper.Map<ContainerTypeDto>(c);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ContainerTypeAppService methyod GetContainerTypeAsync failed for {containerTypeId}, error: {ex.Message}");
                throw;
            }
        }
    }
}
