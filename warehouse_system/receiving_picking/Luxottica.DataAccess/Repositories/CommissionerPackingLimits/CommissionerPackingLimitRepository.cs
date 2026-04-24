using Luxottica.ApplicationServices.Shared.Dto.CommisionerPackingLimits;
using Luxottica.ApplicationServices.Shared.Dto.DivertLines;
using Luxottica.Core.Entities.EXT;
using Luxottica.Core.Entities.HighwayPikingLanes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.DataAccess.Repositories.CommissionerPackingLimits
{
    public class CommissionerPackingLimitRepository : Repository<int, Commissioner_Packing_Limits>
    {
        private readonly E1ExtContext _e1EXtContext;
        private readonly ILogger<CommissionerPackingLimitRepository> _logger;
        private IConfiguration Configuration { get; }
        public CommissionerPackingLimitRepository(V10Context context,E1ExtContext e1EXtContext, IConfiguration configuration, ILogger<CommissionerPackingLimitRepository> logger) : base(context)
        {
            _e1EXtContext = e1EXtContext;
            Configuration = configuration;
            _logger = logger;
        }

        public async Task<List<Commissioner_Packing_Limits>> GetCommissionerPackingLimits()
        {
            try
            {
                return await _e1EXtContext.Commissioner_Packing_Limits.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR GetCommissionerPackingLimits in CommissionerPackingLimitRepository, Message: {ex.Message}.");
                throw new Exception($"Error when retrieving CommissionerPackingLimits in E1_EXT,  {ex.Message}");
            }
        }


        public async Task UpdateCommissionerPackingLimits(CommisionerPackingLimitsRequest commisionerPackingLimitsRequest)
        {
            try
            {
                var limit62 = await _e1EXtContext.Commissioner_Packing_Limits.FirstOrDefaultAsync(limit => limit.PutStationNr == 62);
                var limit61 = await _e1EXtContext.Commissioner_Packing_Limits.FirstOrDefaultAsync(limit => limit.PutStationNr == 61);
                var limit1 = await _e1EXtContext.Commissioner_Packing_Limits.FirstOrDefaultAsync(limit => limit.PutStationNr == 1);
                var limit2 = await _e1EXtContext.Commissioner_Packing_Limits.FirstOrDefaultAsync(limit => limit.PutStationNr == 2);

                if (limit62 != null)
                    limit62.Limit = commisionerPackingLimitsRequest.SetLimitSuresort_1;

                if (limit61 != null)
                    limit61.Limit = commisionerPackingLimitsRequest.SetLimitSuresort_2;

                if (limit1 != null)
                    limit1.Limit = commisionerPackingLimitsRequest.SetLimitPutWall_1;

                if (limit2 != null)
                    limit2.Limit = commisionerPackingLimitsRequest.SetLimitPutWall_2;

                await _e1EXtContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR UpdateCommissionerPackingLimits in CommissionerPackingLimitRepository, Message: {ex.Message}.");
                throw new Exception($"Error when updating CommissionerPackingLimits in E1_EXT,  {ex.Message}");
            }
        }
    }
}
