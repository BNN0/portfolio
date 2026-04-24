using Luxottica.ApplicationServices.Shared.Dto.CommisionerPackingLimits;
using Luxottica.Core.Entities.EXT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.CommissionerPackingLimits
{
    public interface ICommissionerPackingLimitAppService
    {
        Task<List<Commissioner_Packing_Limits>> GetLimitsPacking();
        Task UpdateCommissionerLimits(CommisionerPackingLimitsRequest commisionerPackingLimitsRequest);
    }
}
