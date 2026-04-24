using AutoMapper;
using Luxottica.ApplicationServices.Cameras;
using Luxottica.ApplicationServices.Shared.Dto.CommisionerPackingLimits;
using Luxottica.Core.Entities.Acknowledgments;
using Luxottica.Core.Entities.Cameras;
using Luxottica.Core.Entities.EXT;
using Luxottica.DataAccess.Repositories;
using Luxottica.DataAccess.Repositories.Camera;
using Luxottica.DataAccess.Repositories.CommissionerPackingLimits;
using Luxottica.DataAccess.Repositories.Commissioners;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.CommissionerPackingLimits
{
    public class ComissionerPackingLimitAppService : ICommissionerPackingLimitAppService
    {
        private readonly IRepository<int, Commissioner_Packing_Limits> _repository;
        private readonly CommissionerPackingLimitRepository _commissionerPackingRepository;
        private readonly ILogger<ComissionerPackingLimitAppService> _logger;
        public ComissionerPackingLimitAppService(IRepository<int, Commissioner_Packing_Limits> repository, CommissionerPackingLimitRepository commissionerPackingLimitRepository, ILogger<ComissionerPackingLimitAppService> logger)
        {
            _repository = repository;
            _commissionerPackingRepository = commissionerPackingLimitRepository;
            _logger = logger;
        }


        public async Task<List<Commissioner_Packing_Limits>> GetLimitsPacking()
        {
            try
            {
                var commissionerPackingLimits = await _commissionerPackingRepository.GetCommissionerPackingLimits();
                return commissionerPackingLimits;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR GetLimitsPacking in ComissionerPackingLimitAppService, Message: {ex.Message}.");
                throw new Exception($"{ex.Message}");
            }
        }

        public async Task UpdateCommissionerLimits(CommisionerPackingLimitsRequest commisionerPackingLimitsRequest)
        {
            try
            {
                await _commissionerPackingRepository.UpdateCommissionerPackingLimits(commisionerPackingLimitsRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT Commisioner_Packing_Limits in ComissionerPackingLimitAppService, Message: {ex.Message}.");
                throw new Exception($"{ex.Message}");
            }
        }
    }
}
