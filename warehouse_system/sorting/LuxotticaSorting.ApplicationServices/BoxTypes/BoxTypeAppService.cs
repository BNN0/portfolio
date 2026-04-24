using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.Shared.DTO.BoxTypes;
using LuxotticaSorting.Core.BoxTypes;
using LuxotticaSorting.DataAccess.Repositories.BoxTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.BoxTypes
{
    public class BoxTypeAppService : IBoxTypeAppService
    {
        private readonly IRepository<int, BoxType> _repository;
        private readonly ILogger<BoxTypeAppService> _logger;
        private readonly BoxTypeRepository _BoxTypeRepository;
        public BoxTypeAppService(IRepository<int, BoxType> repository, ILogger<BoxTypeAppService> logger,
    BoxTypeRepository boxTypeRepository)
        {
            _repository = repository;
            _logger = logger;
            _BoxTypeRepository = boxTypeRepository;
        }
        public async Task<int> AddBoxTypeAsync(BoxTypesAddDTO boxTypesAddDTO)
        {
            try
            {
                var entity = await _BoxTypeRepository.AddAsync(boxTypesAddDTO);

                return entity.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError($"BoxTypeAppService method AddBoxTypeAsync failed , erorr: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteBoxTypeAsync(int boxTypeId)
        {
            try
            {
                await _BoxTypeRepository.DeleteAsync(boxTypeId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"BoxTypeAppService method DeleteBoxTypeAsync error occurred while deleting BoxType {boxTypeId}, error: {ex.Message}");
                throw;
            }
        }

        public async Task EditBoxTypeAsync(int id, BoxTypesAddDTO boxTypesAddDTO)
        {
            try
            {
                await _BoxTypeRepository.UpdateAsync(id,boxTypesAddDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError($"BoxTypeAppService method EditBoxTypeAsync error occurred while updating BoxType {id}, error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<BoxType>> GetBoxTypesAsync()
        {
            try
            {
                var boxTypes = await _repository.GetAll().ToListAsync();
                return boxTypes;
            }
            catch (Exception ex)
            {
                _logger.LogError($"BoxTypeAppService method GetBoxTypesAsync error occurred while retrieving BoxTypes, error: {ex.Message}");
                throw;
            }
        }

        public async Task<BoxType> GetBoxTypeByIdAsync(int boxTypeId)
        {
            try
            {
                var boxType = await _repository.GetAsync(boxTypeId);
                if (boxType == null)
                {
                    _logger.LogWarning($"BoxType with ID {boxTypeId} does not exist in BoxTypeAppService method GetBoxTypeByIdAsync");
                    throw new Exception($"BoxType with {boxTypeId} does not exist or null");
                }
                return boxType;
            }
            catch (Exception ex)
            {
                _logger.LogError($"BoxTypeAppService method GetBoxTypeByIdAsync error occurred while retrieving BoxType {boxTypeId}, error: {ex.Message}");
                throw;
            }
        }
    }
}
