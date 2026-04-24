using Luxottica.ApplicationServices.Cameras;
using Luxottica.Core.Entities.Commissioners;
using Luxottica.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.Commissioners
{
    public class CommissionersAppService : ICommissionersAppService
    {
        private readonly IRepository<int, Commissioner> _repository;
        private readonly ILogger<CommissionersAppService> _logger;
        public CommissionersAppService(IRepository<int, Commissioner> repository, ILogger<CommissionersAppService> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        public async Task<List<Commissioner>> GetComissionnersAsync()
        {
            try
            {
                var commissioners = await _repository.GetAll().ToListAsync();
                return commissioners;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT COMMISSIONER in GetComissionnersAsync SERVICE {ex.Message}");
                throw;
            }
        }

        public async Task<Commissioner> GetFirstCommissionerAsync()
        {
            try
            {
                var commissioner = await _repository.GetAll().FirstOrDefaultAsync();
                if (commissioner == null)
                {
                    _logger.LogError($"ERROR SELECT in GetFirstCommissionerAsync SERVICE, Message: No Commissioner record found.");
                    throw new Exception("No Commissioner record found.");
                }

                return commissioner;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT in GetFirstCommissionerAsync SERVICE {ex.Message}");
                throw new Exception($"Error while retrieving Commissioner record: {ex.Message}");
            }
        }

        public async Task UpdateCommissionerAsync(Commissioner commissioner)
        {
            try
            {
                await _repository.UpdateAsync(commissioner);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR UPDATE COMMISSIONER in UpdateCommissionerAsync SERVICE {ex.Message}");
                throw new Exception($"Error while updating Commissioner record: {ex.Message}");
            }
        }



    }
}
