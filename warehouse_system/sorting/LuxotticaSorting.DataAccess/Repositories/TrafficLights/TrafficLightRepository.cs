using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.Shared.DTO.TrafficLights;
using LuxotticaSorting.Core.DivertLanes;
using LuxotticaSorting.Core.MultiBoxWaves;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.DataAccess.Repositories.TrafficLights
{
    public class TrafficLightRepository : Repository<int, MultiBoxWave>
    {
        public TrafficLightRepository(SortingContext context) : base(context)
        {

        }

        public async Task<TrafficLightDataDto> StatusTrafficLigthTruckLine2()
        {
            try
            {
                var trafficLightForLine2 = new TrafficLightDataDto();
                bool divertLane2 = await Context.DivertLanes.Where(x => x.DivertLanes == 2).Select(x => x.Status).FirstOrDefaultAsync();
                if (divertLane2)
                {
                    var divertLane2ContainerID = await Context.DivertLanes.Where(x => x.DivertLanes == 2).Select(x => x.ContainerId).FirstOrDefaultAsync();
                    var containerID = await Context.Containers.Where(x => x.Id == divertLane2ContainerID).Select(x => x.ContainerId).FirstOrDefaultAsync();
                    var boxExistingWCSRouting = await Context.multiBox_Wave.Where(x => (x.Status == null || x.Status == "IN") && x.DivertLane == 2
                    && x.ContainerId == containerID)
                        .ToListAsync() ?? null;

                    if (boxExistingWCSRouting?.Count > 0)
                    {
                        trafficLightForLine2.LigthRed = true;
                        trafficLightForLine2.LigthGreen = false;
                    }
                    else
                    {
                        trafficLightForLine2.LigthRed = false;
                        trafficLightForLine2.LigthGreen = true;
                    }
                }
                else
                {
                    trafficLightForLine2.LigthRed = false;
                    trafficLightForLine2.LigthGreen = false;
                }
                return trafficLightForLine2;
            }
            catch (Exception ex)
            {
                throw new Exception("An internal error has occurred.");
            }
        }

        public async Task<TrafficLightDataDto> StatusTrafficLigthTruckLine4()
        {
            try
            {
                var trafficLightForLine4 = new TrafficLightDataDto();
                bool divertLane4 = await Context.DivertLanes.Where(x => x.DivertLanes == 4).Select(x => x.Status).FirstOrDefaultAsync();
                if (divertLane4)
                {
                    var divertLane4ContainerID = await Context.DivertLanes.Where(x => x.DivertLanes == 4).Select(x => x.ContainerId).FirstOrDefaultAsync();
                    var containerID = await Context.Containers.Where(x => x.Id == divertLane4ContainerID).Select(x => x.ContainerId).FirstOrDefaultAsync();
                    var boxExistingWCSRouting = await Context.multiBox_Wave.Where(x => (x.Status == null || x.Status == "IN") && x.DivertLane == 4
                    && x.ContainerId == containerID)
                        .ToListAsync() ?? null;

                    if (boxExistingWCSRouting?.Count > 0)
                    {
                        trafficLightForLine4.LigthRed = true;
                        trafficLightForLine4.LigthGreen = false;
                    }
                    else
                    {
                        trafficLightForLine4.LigthRed = false;
                        trafficLightForLine4.LigthGreen = true;
                    }
                }
                else
                {
                    trafficLightForLine4.LigthRed = false;
                    trafficLightForLine4.LigthGreen = false;
                }
                return trafficLightForLine4;
            }
            catch (Exception ex)
            {
                throw new Exception("An internal error has occurred.");
            }

        }
    }
}
