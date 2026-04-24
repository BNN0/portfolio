using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.BoxTypes;
using LuxotticaSorting.ApplicationServices.Shared.DTO.BoxTypes;
using LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodes;
using LuxotticaSorting.Core.BoxTypes;
using LuxotticaSorting.Core.CarrierCodes;
using LuxotticaSorting.DataAccess.Repositories.BoxTypes;
using LuxotticaSorting.DataAccess.Repositories.CarrierCodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.CarriersCodes
{
    public class CarrierCodeAppService : ICarrierCodeAppService
    {
        private readonly IRepository<int, CarrierCode> _repository;
        private readonly ILogger<CarrierCodeAppService> _logger;
        private readonly CarrierCodeRepository _CarrierCodeRepository;
        public CarrierCodeAppService(IRepository<int, CarrierCode> repository, ILogger<CarrierCodeAppService> logger,
    CarrierCodeRepository carrierCodeRepository)
        {
            _repository = repository;
            _logger = logger;
            _CarrierCodeRepository = carrierCodeRepository;
        }


        public async Task<int> AddCarrierCodeAsync(CarrierCodeAddDto carrierCodeAddDto)
        {
            try
            {
                var entity = await _CarrierCodeRepository.AddAsync(carrierCodeAddDto);
                return entity.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError($"CarrierCodeAppService method AddCarrierCodeAsync error has occurred, error: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteCarrierCodeAsync(int carrierCodeId)
        {
            try
            {
                await _CarrierCodeRepository.DeleteAsync(carrierCodeId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CarrierCodeAppService method DeleteCarrierCodeAsync error occurred while deleting CarrierCode {carrierCodeId}, error: {ex.Message}");
                throw;
            }
        }

        public async Task EditCarrierCodeAsync(int id, CarrierCodeAddDto carrierCodeAddDto)
        {
            try
            {
                await _CarrierCodeRepository.UpdateAsync(id,carrierCodeAddDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CarrierCodeAppService method EditCarrierCodeAsync error occurred while updating CarrierCode {id}, error: {ex.Message}");
                throw;
            }
        }

        public async Task<CarrierCode> GetCarrierCodeByIdAsync(int carrierCodeId)
        {
            try
            {
                var carrierCode = await _repository.GetAsync(carrierCodeId);
                if (carrierCode == null)
                {
                    _logger.LogWarning($"CarrierCode with ID {carrierCodeId} does not exist in CarrierCodeAppService method GetCarrierCodeByIdAsync");
                    throw new Exception($"CarrierCode with ID {carrierCodeId} does not exist.");
                }
                return carrierCode;
            }
            catch (Exception ex)
            {
                _logger.LogError($"CarrierCodeAppService method GetCarrierCodeByIdAsync error occurred while retrieving CarrierCode {carrierCodeId}, error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<CarrierCode>> GetCarrierCodesAsync()
        {
            try
            {
                var carrierCodes = await _repository.GetAll().ToListAsync();
                return carrierCodes;
            }
            catch (Exception ex)
            {
                _logger.LogError($"CarrierCodeAppService method GetCarrierCodesAsync error occurred while retrieving CarrierCodes, error: {ex.Message}");
                throw;
            }
        }
    }
}
