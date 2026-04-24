using LuxotticaSorting.ApplicationServices.Shared.DTO.TrafficLights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.TrafficLights
{
    public interface ITrafficLightAppService
    {
        Task<TrafficLightDataDto> GetStatusLightLine2();
        Task<TrafficLightDataDto> GetStatusLightLine4();
    }
}
