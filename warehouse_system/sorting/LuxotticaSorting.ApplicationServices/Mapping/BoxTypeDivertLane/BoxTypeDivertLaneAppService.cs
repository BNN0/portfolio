using AutoMapper;
using LuxotticaSorting.ApplicationServices.Shared.DTO.BoxTypeDivertLaneMapping;
using LuxotticaSorting.Core.Mapping.BoxTypeDivertLane;
using LuxotticaSorting.DataAccess.Repositories.Mapping.BoxTypeDivertLane;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Mapping.BoxTypeDivertLane
{
    public class BoxTypeDivertLaneAppService : IBoxTypeDivertLaneAppService
    {
        private readonly BoxTypeDivertLaneRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<BoxTypeDivertLaneAppService> _logger;

        public BoxTypeDivertLaneAppService(BoxTypeDivertLaneRepository repository, IMapper mapper, ILogger<BoxTypeDivertLaneAppService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<BoxTypeDivertLaneMapping> AddBoxTypeDivertLaneMappingAsync(BoxTypeDivertLaneMappingAddDto entity)
        {
            try
            {
                var m = _mapper.Map<BoxTypeDivertLaneMapping>(entity);
                var result = await _repository.AddAsync(m);

                if (result == null)
                {
                    throw new Exception("Error to added Jackpot Line");
                }
                return result;

            }
            catch (Exception ex)
            {
                _logger.LogError($"BoxTypeDivertLaneAppService Failed to add new Mapping BoxType-DivertLane, error: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteBoxTypeDivertLaneMappingAsync(int id)
        {
            try
            {
                var result = await _repository.DeleteAsync(id);

                if(!result)
                {
                    throw new Exception("Not found or null");
                }
                return result;
                
            }
            catch (Exception ex)
            {
                _logger.LogError($"BoxTypeDivertLaneAppService method DeleteBoxTypeDivertLaneMappingAsync failed to delete register with {id}, error: {ex.Message}");
                throw;
            }
        }

        public async Task<BoxTypeDivertLaneMapping> EditBoxTypeDivertLaneMappingAsync(int id, BoxTypeDivertLaneMappingAddDto entity)
        {
            try
            {
                var m = _mapper.Map<BoxTypeDivertLaneMapping>(entity);
                m.Id = id;
                var result = await _repository.UpdateAsync(m);
                if(result == null)
                {
                    throw new Exception("Not found or null");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"BoxTypeDivertLaneAppService method EditBoxTypeDivertLaneMappingAsync failed to edit Mapping BoxType-DivertLane with {id}, error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<BoxTypeDivertLaneMenuView>> GetAllBoxTypeDivertLaneViewAsync()
        {
            try
            {
                var result = await _repository.GetAllView();
                if (result == null)
                {
                    throw new Exception("Error in obtaining information");
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"BoxTypeDivertLaneAppService method GetAllBoxTypeDivertLaneViewAsync failed to retrieve Mappings BoxType-DivertLane View, error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<BoxTypeDivertLaneMapping>> GetAllBoxTypeDivertLaneMappingsAsync()
        {
            try
            {
                var result = await _repository.GetAll().ToListAsync();

                if(result == null || result.Count <= 0)
                {
                    throw new Exception("Error in obtaining information");
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"BoxTypeDivertLaneAppService mehtod GetAllBoxTypeDivertLaneMappingsAsync failed to retrieve Mappings BoxType-DivertLane, error: {ex.Message}");
                throw;
            }
        }

        public async Task<BoxTypeDivertLaneMapping> GetBoxTypeDivertLaneMappingAsync(int id)
        {
            try
            {
                var result = await _repository.GetAsync(id);

                if(result == null)
                {
                    throw new Exception("Error in obtaining information");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"BoxTypeDivertLaneAppService mehtod GetBoxTypeDivertLaneMappingAsync failed to retrieve Mapping BoxType-DivertLane with {id}, error: {ex.Message}");
                throw;
            }
        }
    }
}
