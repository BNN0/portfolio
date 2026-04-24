using AutoMapper;
using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.DivertLanes;
using LuxotticaSorting.ApplicationServices.Shared.DTO.Containers;
using LuxotticaSorting.ApplicationServices.Shared.DTO.DivertLanes;
using LuxotticaSorting.Core.Container;
using LuxotticaSorting.Core.DivertLanes;
using LuxotticaSorting.DataAccess.Repositories.Containers;
using LuxotticaSorting.DataAccess.Repositories.DivertLanes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Containers
{
    public class ContainerAppService : IContainerAppService
    {
        private readonly IRepository<int, ContainerTable> _repository;
        private readonly ILogger<ContainerAppService> _logger;
        private readonly ContainersRepository _containersRepository;
        private readonly IMapper _mapper;
        public ContainerAppService(IRepository<int, ContainerTable> repository, ILogger<ContainerAppService> logger,
            ContainersRepository containersRepository, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _containersRepository = containersRepository;
            _mapper = mapper;
        }

        public async Task<int> AddContainerAsync(ContainerAddDTO containerAddDTO)
        {
            try
            {
                var c = _mapper.Map<ContainerTable>(containerAddDTO);
                var id = await _containersRepository.AddAsync(c);
                return id.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ContainerAppService method AddContainerAsync failed, error: {ex.Message}");
                throw;
            }
        }


        public async Task<bool> AddContainerToPrintAsync(ContainerToPrint containerAddDTO)
        {
            try
            {
                var id = await _containersRepository.ContainerToPrint(containerAddDTO);
                return id;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ContainerAppService method AddContainerToPrintAsync failed, error: {ex.Message}");
                throw;
            }
        }


        public async Task<int> AddContainerGayLordAsync(ContainerAddOneStepDTO containerAddOneStepDTO)
        {
            try
            {
                var c = _mapper.Map<ContainerTable>(containerAddOneStepDTO);
                var id = await _containersRepository.AddAsyncGayLord(c);
                return id.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ContainerAppService method AddContainerGayLordAsync failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task<int> AddContainerTruckAsync(ContainerAddOneStepDTO containerAddOneStepDTO)
        {
            try
            {
                var c = _mapper.Map<ContainerTable>(containerAddOneStepDTO);
                var id = await _containersRepository.AddAsyncTruck(c);
                return id.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ContainerAppService method AddContainerTruckAsync failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteContainerAsync(int containerId)
        {
            try
            {
                await _containersRepository.DeleteAsync(containerId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ContainerAppService method DeleteContainerAsync error occurred while deleting Container {containerId}, error: {ex.Message}");
                throw;
            }
        }

        public async Task EditContainerAsync(int id, ContainerAddDTO containerAddDTO)
        {
            try
            {
                var c = _mapper.Map<ContainerTable>(containerAddDTO);
                c.Id = id;
                await _containersRepository.UpdateAsync(c);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ContainerAppService method EditContainerAsync failed for {id}, error: {ex.Message}");
                throw;
            }
        }

        public async Task<ContainerDTO> GetContainerByIdAsync(int containerId)
        {
            try
            {
                var c = await _repository.GetAsync(containerId);
                ContainerDTO dto = _mapper.Map<ContainerDTO>(c);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ContainerAppService method GetContainerByIdAsync failed for {containerId}, error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<ContainerDTO>> GetContainersAsync()
        {
            try
            {
                var c = await _repository.GetAll()
                 .OrderBy(entity => entity.Id)
        .ToListAsync();
                List<ContainerDTO> containerDTOs = _mapper.Map<List<ContainerDTO>>(c);
                return containerDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ContainerAppService method GetContainersAsync failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<ContainerToShow>> GetContainersTruckAsync()
        {
            try
            {
                var c = await _containersRepository.GetTrucks();
                List<ContainerToShow> containerDTOs = _mapper.Map<List<ContainerToShow>>(c);
                return containerDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ContainerAppService method GetContainersTruckAsync failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<ContainerToShow>> GetContainersGaylordAsync()
        {
            try
            {
                var c = await _containersRepository.GetGaylord();
                List<ContainerToShow> containerDTOs = _mapper.Map<List<ContainerToShow>>(c);
                return containerDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ContainerAppService method GetContainersTruckAsync failed, error: {ex.Message}");
                throw;
            }
        }
    }
}
