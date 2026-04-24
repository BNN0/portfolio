using Luxottica.ApplicationServices.Shared.Dto.CameraAssignments;
using Luxottica.ApplicationServices.Shared.Dto.DivertLines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.DivertLines
{
    public interface IDivertLineService
    {
        Task<List<DivertLineDto>> GetDivertLineAsync();

        Task AddDivertLineAsync(DivertLineAddDto divertLine);

        Task DeleteDivertLineAsync(int divertLineId);

        Task<DivertLineDto> GetDivertLineByIdAsync(int divertLineId);

        Task EditDivertLineAsync(int id, DivertLineAddDto divertLine);
    }
}
