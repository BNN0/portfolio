using Luxottica.ApplicationServices.Shared.Dto.MapPhysicVirtualSAP;
using Luxottica.Core.Entities.PhysicalMaps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.PhysicalMaps
{
    public interface IMapPhysicalAppService
    {
        Task<List<MapPhysicVirtualSAPDto>> GetMapPhysicalAsync();

        Task AddMapPhysicalAsync(MapPhysicVirtualSAPAddDto mapPhysical);

        Task DeleteMapPhysicalAsync(int mapPhysicalId);

        Task<MapPhysicVirtualSAPDto> GetMapPhysicalByIdAsync(int mapPhysicalId);

        Task EditMapPhysicalAsync(int id, MapPhysicVirtualSAPAddDto mapPhysical);

        int GetDivertJackpot();

        Task<int> UpdateValueDiverJKMaps(int oldJackpotId);

        Task<List<MapPhysicalGetAllDto>> GetlistFilterByIdDiverLine(int mapDiverlineId);
        Task<List<MapPhysicalGetAllDto>> GetMapPhysicalAllNew();
        Task<int> GetJackpotAssignment();
    }
}
