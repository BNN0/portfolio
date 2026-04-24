using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodeLogisticAgent;
using LuxotticaSorting.Core.LogisticAgents;
using LuxotticaSorting.Core.Mapping.CarrierCodeDivertLine;
using LuxotticaSorting.Core.Mapping.CarrierCodeLogisticAgent;
using Microsoft.EntityFrameworkCore;

namespace LuxotticaSorting.DataAccess.Repositories.Mapping.CarrierCodeLogisticAgent
{
    public class CarrierCodeLogisticAgentRepository : Repository<int, CarrierCodeLogisticAgentMapping>
    {
        public CarrierCodeLogisticAgentRepository(SortingContext context) : base(context)
        {
        }

        public override async Task<CarrierCodeLogisticAgentMapping> AddAsync(CarrierCodeLogisticAgentMapping carrierCodeLogisticAgentMapping)
        {
            var existingMapping = await Context.CarrierCodeLogisticAgentMappings
        .FirstOrDefaultAsync(x => x.CarrierCodeId == carrierCodeLogisticAgentMapping.CarrierCodeId);

            if (existingMapping != null)
            {
                throw new InvalidOperationException("Multiple Carrier Codes can have only one Logistic Agent.");
            }

            var evistingCarrier = await Context.CarrierCodes
                .FirstOrDefaultAsync(x => x.Id == carrierCodeLogisticAgentMapping.CarrierCodeId);

            if (evistingCarrier == null)
            {
                throw new InvalidDataException("The provided carrier code does not exist.");
            }
            var evistingLogistic = await Context.LogisticAgents
                .FirstOrDefaultAsync(x => x.Id == carrierCodeLogisticAgentMapping.LogisticAgentId);

            if (evistingLogistic == null)
            {
                throw new InvalidDataException("The provided logistic agent does not exist.");
            }

            await Context.CarrierCodeLogisticAgentMappings.AddAsync(carrierCodeLogisticAgentMapping);
            await Context.SaveChangesAsync();

            return carrierCodeLogisticAgentMapping;
        }

        public override async Task<CarrierCodeLogisticAgentMapping> UpdateAsync(CarrierCodeLogisticAgentMapping carrierCodeLogisticAgentMapping)
        {
            var entity = await Context.CarrierCodeLogisticAgentMappings.FirstOrDefaultAsync(x => x.Id == carrierCodeLogisticAgentMapping.Id);

            if (entity == null)
            {
                throw new Exception("The mapping between Carrier Code and Logistic Agent does not exist.");
            }

            var existingMapping = await Context.CarrierCodeLogisticAgentMappings.FirstOrDefaultAsync(x =>
                x.Id != entity.Id &&
                x.CarrierCodeId == carrierCodeLogisticAgentMapping.CarrierCodeId);

            if (existingMapping != null)
            {
                throw new Exception("Multiple Carrier Codes cannot belong to the same Logistic Agent.");
            }

            var evistingCarrier = await Context.CarrierCodes
                .FirstOrDefaultAsync(x => x.Id == carrierCodeLogisticAgentMapping.CarrierCodeId);

            if (evistingCarrier == null)
            {
                throw new InvalidDataException("The provided carrier code does not exist.");
            }
            var evistingLogistic = await Context.LogisticAgents
                .FirstOrDefaultAsync(x => x.Id == carrierCodeLogisticAgentMapping.LogisticAgentId);

            if (evistingLogistic == null)
            {
                throw new InvalidDataException("The provided logistic agent does not exist.");
            }

            // Check if there is a CarrierCodeDivertLaneMapping with the same CarrierCodeId and Status = true
            var existingDivertLaneMapping = await Context.CarrierCodeDivertLaneMappings.FirstOrDefaultAsync(x =>
                x.CarrierCodeId == carrierCodeLogisticAgentMapping.CarrierCodeId && x.Status);

            if (existingDivertLaneMapping != null)
            {
                throw new Exception("Cannot update the mapping because there is an active CarrierCodeDivertLaneMapping with the same CarrierCodeId.");
            }

            //if (entity.LogisticAgentId != carrierCodeLogisticAgentMapping.LogisticAgentId)
            //{
            //    var existingSorter = await Context.MappingSorters.FirstOrDefaultAsync(x => x.LogisticAgentId == entity.LogisticAgentId);
            //    if (existingSorter != null)
            //    {
            //        await updateLogisticAgent(carrierCodeLogisticAgentMapping.LogisticAgentId, carrierCodeLogisticAgentMapping.CarrierCodeId);
            //    }
            //    entity.LogisticAgentId = carrierCodeLogisticAgentMapping.LogisticAgentId;
            //}


            entity.CarrierCodeId = carrierCodeLogisticAgentMapping.CarrierCodeId;
            entity.LogisticAgentId = carrierCodeLogisticAgentMapping.LogisticAgentId;

            await Context.SaveChangesAsync();

            return entity;
        }

        public async Task<List<CarrierCodeLogisticAgentGetAllDto>> GetCombinedDataAsync()
        {
            var combinedDataList = await (
                from mapping in Context.CarrierCodeLogisticAgentMappings
                join carrierCode in Context.CarrierCodes on mapping.CarrierCodeId equals carrierCode.Id
                join logisticAgent in Context.LogisticAgents on mapping.LogisticAgentId equals logisticAgent.Id
                select new CarrierCodeLogisticAgentGetAllDto
                {
                    Id = mapping.Id,
                    CarrierCodeId = carrierCode.Id,
                    CarrierCodes = carrierCode.CarrierCodes,
                    LogisticAgentId = logisticAgent.Id,
                    LogisticAgents = logisticAgent.LogisticAgents
                }
            ).ToListAsync();
            if (combinedDataList == null)
            {
                return null;
            }
            return combinedDataList;
        }

        public override async Task<CarrierCodeLogisticAgentMapping> DeleteAsync(int id)
        {
            var entity = await Context.CarrierCodeLogisticAgentMappings.FindAsync(id);

            if (entity == null)
            {
                throw new Exception("The mapping between Carrier Code and Logistic Agent does not exist.");
            }
            //var dataCarrierCode = await Context.CarrierCodeDivertLaneMappings.FirstOrDefaultAsync(x => x.CarrierCodeId == entity.CarrierCodeId);
            // var divertLaneData = await Context.DivertLanes.FirstOrDefaultAsync(x => x.Id == dataCarrierCode.DivertLaneId);
            // if(divertLaneData.Status == true)
            // {
            //     throw new Exception("The record cannot be deleted because the line is active.");
            // }

            // Check if there is a CarrierCodeDivertLaneMapping with the same CarrierCodeId and Status = true
            var existingDivertLaneMapping = await Context.CarrierCodeDivertLaneMappings.FirstOrDefaultAsync(x =>
                x.CarrierCodeId == entity.CarrierCodeId && x.Status);

            if (existingDivertLaneMapping != null)
            {
                throw new Exception("Cannot delete the mapping because there is an active CarrierCodeDivertLaneMapping with the same CarrierCodeId.");
            }

            //var existingSorter = await Context.MappingSorters.FirstOrDefaultAsync(x => x.LogisticAgentId == entity.LogisticAgentId);
            //if (existingSorter != null)
            //{
            //    await deleteLogisticAgent(entity.LogisticAgentId, entity.CarrierCodeId);
            //}

            Context.CarrierCodeLogisticAgentMappings.Remove(entity);
            await Context.SaveChangesAsync();

            return entity;
        }

        private async Task updateLogisticAgent(int logisticAgent, int carriercode)
        {
            var mapping = await Context.CarrierCodeDivertLaneMappings.FirstOrDefaultAsync(x => x.CarrierCodeId == carriercode);
            var entity = await Context.MappingSorters.FirstOrDefaultAsync(x => x.DivertLaneId == mapping.DivertLaneId);
            if (entity.LogisticAgentId == null)
            {
                entity.LogisticAgentId = logisticAgent;
            }
            await Context.SaveChangesAsync();
        }

        private async Task deleteLogisticAgent(int logisticAgent, int carriercode)
        {
            var mapping = await Context.CarrierCodeDivertLaneMappings.FirstOrDefaultAsync(x => x.CarrierCodeId == carriercode);
            var entity = await Context.MappingSorters.FirstOrDefaultAsync(x => x.DivertLaneId == mapping.DivertLaneId);
            if (entity.CarrierCodeId.Length == 1)
            {
                entity.LogisticAgentId = null;
            }
            await Context.SaveChangesAsync();
        }
    }
}
