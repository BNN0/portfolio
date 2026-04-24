using Luxottica.ApplicationServices.Shared.Dto.HighwayPickingLanes;
using Luxottica.ApplicationServices.Shared.Dto.LimitSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.HighwayPickingLanes
{
    public interface IHighWayPickingLanesAppService
    {
        Task<List<HighwayPickingLanesDTO>> GetHighwayPickingLaneAsync();
        Task<HighwayPickingRequest> UpdateLimits(HighwayPickingRequest request);
        Task AddHighwayAsync();
    }
}
