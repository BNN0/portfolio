using LuxotticaSorting.ApplicationServices.Shared.DTO.BoxTypeDivertLaneMapping;
using LuxotticaSorting.Core.Mapping.BoxTypeDivertLane;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Mapping.BoxTypeDivertLane
{
    public interface IBoxTypeDivertLaneAppService
    {
        Task<List<BoxTypeDivertLaneMapping>> GetAllBoxTypeDivertLaneMappingsAsync();
        Task<BoxTypeDivertLaneMapping> GetBoxTypeDivertLaneMappingAsync(int id);
        Task<BoxTypeDivertLaneMapping> AddBoxTypeDivertLaneMappingAsync(BoxTypeDivertLaneMappingAddDto entity);
        Task<BoxTypeDivertLaneMapping> EditBoxTypeDivertLaneMappingAsync(int id, BoxTypeDivertLaneMappingAddDto entity);
        Task<bool> DeleteBoxTypeDivertLaneMappingAsync(int id);
        Task<List<BoxTypeDivertLaneMenuView>> GetAllBoxTypeDivertLaneViewAsync();
    }
}
