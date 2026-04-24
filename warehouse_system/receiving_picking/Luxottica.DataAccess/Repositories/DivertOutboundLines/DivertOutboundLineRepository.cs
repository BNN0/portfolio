using Luxottica.ApplicationServices.Shared.Dto.DivertOutboundLines;
using Luxottica.Core.Entities.DivertOutboundLines;
using Microsoft.EntityFrameworkCore;

namespace Luxottica.DataAccess.Repositories.DivertOutboundLines
{
    public class DivertOutboundLineRepository : Repository<int, DivertOutboundLine>
    {
        public DivertOutboundLineRepository(V10Context context) : base(context)
        {

        }

        public async Task<DivertOutboundLine> AddAsync(DivertOutboundLine divertOutboundLine)
        {
            try
            {
                if (Context.DivertOutboundLines.Any())
                {
                    var comissionner = await Context.Commissioners.FirstOrDefaultAsync();
                    divertOutboundLine.CommissionerId = comissionner.Id;
                    await Context.DivertOutboundLines.AddAsync(divertOutboundLine);
                    await Context.SaveChangesAsync();
                }
                return divertOutboundLine;
            }
            catch (Exception)
            {
                throw new Exception("error when adding divertOutbound limits");
            }

        }


        public async Task<DivertOutboundLineRequestDto> GetCommissionerPresentLimits ()
        {
            try
            {
                var entity = await Context.DivertOutboundLines.Skip(1).FirstOrDefaultAsync();
                var response = new DivertOutboundLineRequestDto
                {
                    LimitDivertOutbound = entity.MultiTotes,
                    LimitLPTUMachine1 = entity.MaxTotesLPTUMachine1,
                    LimitSPTAMachine1 = entity.MaxTotesSPTAMachine1,
                    LimitSPTAMachine2 = entity.MaxTotesSPTAMachine2,
                };

                return response;
            }
            catch (Exception)
            {
                throw new Exception("Error in obtaining divertOutbound limits when the commissioner is present");
            }
        }

        public async Task<DivertOutboundLineRequestDto> GetLimits()
        {
            try
            {
                var entity = await Context.DivertOutboundLines.FirstOrDefaultAsync();
                var response = new DivertOutboundLineRequestDto
                {
                    LimitDivertOutbound = entity.MultiTotes,
                    LimitLPTUMachine1 = entity.MaxTotesLPTUMachine1,
                    LimitSPTAMachine1 = entity.MaxTotesSPTAMachine1,
                    LimitSPTAMachine2 = entity.MaxTotesSPTAMachine2,
                };
                return response;
            }
            catch (Exception)
            {
                throw new Exception("Error in obtaining divertOutbound limits when the commissioner is absent");
            }
        }

        public async Task UpdateLimits(DivertOutboundLineRequestDto request)
        {
            if (request.LimitDivertOutbound <= 0)
            {
                throw new InvalidOperationException("The LimitDivertOutbound must be greater than 0.");
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
                var entity = await Context.DivertOutboundLines.FirstOrDefaultAsync();
                if (entity == null)
                {
                    throw new InvalidOperationException("The divertOutboundLine does not exist in the database..");
                }
                entity.MultiTotes = request.LimitDivertOutbound;
                entity.MaxTotesLPTUMachine1 = request.LimitLPTUMachine1;
                entity.MaxTotesSPTAMachine1 = request.LimitSPTAMachine1;
                entity.MaxTotesSPTAMachine2 = request.LimitSPTAMachine2;

                await Context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw new InvalidOperationException($"Error when updating limits with commissioner absent  ");
            }
        }

        public async Task UpdateLimitsPresent(DivertOutboundLineRequestDto request)
        {
            if (request.LimitDivertOutbound <= 0)
            {
                throw new InvalidOperationException("The LimitDivertOutbound must be greater than 0.");
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
                var entity = await Context.DivertOutboundLines.Skip(1).FirstOrDefaultAsync();
                if (entity == null)
                {
                    throw new InvalidOperationException("The divertOutboundLine does not exist in the database..");
                }
                entity.MultiTotes = request.LimitDivertOutbound;
                entity.MaxTotesLPTUMachine1 = request.LimitLPTUMachine1;
                entity.MaxTotesSPTAMachine1 = request.LimitSPTAMachine1;
                entity.MaxTotesSPTAMachine2 = request.LimitSPTAMachine2;

                await Context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw new InvalidOperationException($"Error when updating the limits with commissioner present");
            }
        }

    }
}
