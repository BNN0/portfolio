using Luxottica.ApplicationServices.Shared.Dto.HighwayPickingLanes;
using Luxottica.Core.Entities.DivertLines;
using Luxottica.Core.Entities.HighwayPikingLanes;
using Luxottica.Core.Entities.LimitsSettings;
using Luxottica.Core.Entities.ToteHdrs;
using Luxottica.DataAccess.Repositories.LimitSettings;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.DataAccess.Repositories.HighwayPickingLanes
{
    public class HighwayPickingLaneRepository : Repository<int, Core.Entities.HighwayPikingLanes.HighwayPikingLane>
    {
        public HighwayPickingLaneRepository(V10Context context) : base(context)
        {

        }

        public async Task<HighwayPikingLane> AddAsync(HighwayPikingLane highwayPikingLane)
        {
            try
            {
                if (Context.HighwayPikingLanes.Any())
                {
                    var comissionner = await Context.Commissioners.FirstOrDefaultAsync();
                    highwayPikingLane.CommissionerId = comissionner.Id;
                    await Context.HighwayPikingLanes.AddAsync(highwayPikingLane);
                    await Context.SaveChangesAsync();
                }
                return highwayPikingLane;
            }
            catch (Exception)
            {
                throw new Exception("error when adding highwayPikingLane limits");
            }
        }


        public async Task UpdateLimits (HighwayPickingRequest request)
        {
            if (request.LimitHighway <= 0)
            {
                throw new InvalidOperationException("The LimitHighway must be greater than 0.");
            }
            if (request.LimitLPTUMachine1 <= 0)
            {
                throw new InvalidOperationException("The LimitLPTUMachine1 must be greater than 0.");
            }
            if (request.LimitSPTAMachine1 <= 0)
            {
                throw new InvalidOperationException("The LimitSPTAMachine1 must be greater than 0.");
            }
            if (request.LimitSPTAMachine2 <= 0)
            {
                throw new InvalidOperationException("The LimitSPTAMachine2 must be greater than 0.");
            }
            try
            {
                var entity = await Context.HighwayPikingLanes.FirstOrDefaultAsync();
                if (entity == null)
                {
                    throw new InvalidOperationException("The HighwayPickingLane does not exist in the database..");
                }
                entity.MultiTotes = request.LimitHighway;
                entity.MaxTotesLPTUMachine1 = request.LimitLPTUMachine1;
                entity.MaxTotesSPTAMachine1 = request.LimitSPTAMachine1;
                entity.MaxTotesSPTAMachine2 = request.LimitSPTAMachine2;

                await Context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error processing request: {ex.Message}");
            }
        }

    }
}
