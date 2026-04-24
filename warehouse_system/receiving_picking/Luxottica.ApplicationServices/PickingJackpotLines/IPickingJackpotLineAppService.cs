using Luxottica.ApplicationServices.Shared.Dto.PickingJackpotLines;
using Luxottica.Core.Entities.JackpotLines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.PickingJackpotLines
{
    public interface IPickingJackpotLineAppService
    {
        Task<List<PickingJackpotLineGetDto>> GetPickingJackpotLinesAsync();
        Task<PickingJackpotLineGetDto> GetPickingJackpotLineAsync(int id);
        Task<PickingJackpotLine> DeletePickingJackpotLineAsync(int id);

        Task<bool> ChangeDivertLine(int id);
    }
}
