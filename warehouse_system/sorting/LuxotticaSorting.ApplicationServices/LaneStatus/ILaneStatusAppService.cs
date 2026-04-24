using LuxotticaSorting.ApplicationServices.Shared.DTO.LaneStatus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.LaneStatus
{
    public interface ILaneStatusAppService
    {
        public Task UpdateLaneStatus(LaneStatusDto laneStatus);
    }
}
