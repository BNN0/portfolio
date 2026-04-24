using AutoMapper;
using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.Shared.DTO.RecirculationLimits;
using LuxotticaSorting.Core.RecirculationLimits;
using LuxotticaSorting.DataAccess.Repositories.RecirculationLimits;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.RecirculationLimits
{
    public class RecirculationLimitAppService : IRecirculationLimitAppService
    {
        private readonly IRepository<int, RecirculationLimit> _repository;
        private readonly IMapper _mapper;
        private readonly RecirculationLimitRepository _recirculationRepo;
        private readonly ILogger<RecirculationLimitAppService> _logger;
        public RecirculationLimitAppService(IRepository<int, RecirculationLimit> repository, IMapper mapper, RecirculationLimitRepository recirculationRepo, 
            ILogger<RecirculationLimitAppService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _recirculationRepo = recirculationRepo;
            _logger = logger;
        }
        public async Task EditRecirculationLimitAddDto(RecirculationLimitAddDto recirculationLimitAdd)
        {
            try
            {
                var r = _mapper.Map<RecirculationLimit>(recirculationLimitAdd);
                await _recirculationRepo.UpdateAsync(r);
            }
            catch (Exception ex)
            {
                _logger.LogError($"RecirculationLimitAppService method EditRecirculationLimitAddDto failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<RecirculationLimitDto>> GetRecirculationLimitValue()
        {
            try
            {
                var r = await _recirculationRepo.GetValues();
                return r;
            }
            catch (Exception ex)
            {
                _logger.LogError($"RecirculationLimitAppService method GetRecirculationLimitValue failed, error: {ex.Message}");
                throw;
            }
        }
    }
}
