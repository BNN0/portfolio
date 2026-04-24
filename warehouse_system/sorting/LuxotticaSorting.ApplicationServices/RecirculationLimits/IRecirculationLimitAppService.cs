using LuxotticaSorting.ApplicationServices.Shared.DTO.RecirculationLimits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.RecirculationLimits
{
    public interface IRecirculationLimitAppService
    {
        Task<List<RecirculationLimitDto>> GetRecirculationLimitValue();
        Task EditRecirculationLimitAddDto(RecirculationLimitAddDto recirculationLimitAdd);
    }
}
