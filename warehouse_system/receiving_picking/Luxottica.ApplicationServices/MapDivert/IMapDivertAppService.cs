using Luxottica.ApplicationServices.Shared.Dto.MapDivert;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.MapDivert
{
    public interface IMapDivertAppService
    {
        Task<bool> AssignVirtualZones(int id, VectorModel vector);
        Task<bool> DisAssignVirtualZones(int id);
    }
}
