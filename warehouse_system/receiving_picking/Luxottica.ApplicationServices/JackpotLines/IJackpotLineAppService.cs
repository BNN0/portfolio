using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luxottica.ApplicationServices.Shared.Dto.JackpotLines;

namespace Luxottica.ApplicationServices.JackpotLines
{
    public interface IJackpotLineAppService
    {
        Task<List<JackpotLineDto>> GetJackpotLinesAsync();

        Task AddJackpotLinesAsync(JackpotLineAddDto line);
        Task UpdateJackpotState(JackpotLineAddDto line);

        Task DeleteJackpotLinesAsync(int lineId);

        Task<JackpotLineDto> GetJackpotLineByIdAsync(int lineId);

        Task EditJackpotLinesAsync(int id, JackpotLineAddDto line);
        Task<bool> ChangeDivertline(int id);
    }
}
