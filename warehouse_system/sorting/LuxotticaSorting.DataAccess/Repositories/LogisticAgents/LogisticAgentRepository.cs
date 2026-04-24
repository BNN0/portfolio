using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.Core.ContainerTypes;
using LuxotticaSorting.Core.LogisticAgents;
using Microsoft.EntityFrameworkCore;

namespace LuxotticaSorting.DataAccess.Repositories.LogisticAgents
{
    public class LogisticAgentRepository : Repository<int, LogisticAgent>
    {
        public LogisticAgentRepository(SortingContext context) : base(context)
        {
            
        }

        public override async Task<LogisticAgent> AddAsync(LogisticAgent logisticAgent)
        {
            var entity = await Context.LogisticAgents.FirstOrDefaultAsync(x => x.LogisticAgents == logisticAgent.LogisticAgents);
            if (entity != null)
            {
                throw new InvalidOperationException("The logistica Agent value already exists.");
            }

            await Context.LogisticAgents.AddAsync(logisticAgent);
            await Context.SaveChangesAsync();

            return logisticAgent;
        }

        public override async Task<LogisticAgent> UpdateAsync(LogisticAgent logisticAgent)
        {
            var entity = await Context.LogisticAgents.FindAsync(logisticAgent.Id);
            if (entity == null)
            {
                throw new Exception("The logistic Agent does not exist.");
            }

            var existingEntity = await Context.LogisticAgents.FirstOrDefaultAsync(x => x.LogisticAgents == logisticAgent.LogisticAgents);
            if (existingEntity != null && entity.LogisticAgents != logisticAgent.LogisticAgents)
            {
                throw new Exception("The Logistic Agent value already exists.");
            }

            var existingRelation1Entity = await Context.CarrierCodeLogisticAgentMappings.FirstOrDefaultAsync(x => x.LogisticAgentId == logisticAgent.Id);
            if (existingRelation1Entity != null)
            {
                throw new Exception("This LogisticAgent is mapped to a Carrier Code, please delete that record first in order to update this LogisticAgent.");
            }
            var existingRealtion2Entity = await Context.MappingSorters.FirstOrDefaultAsync(x => x.LogisticAgentId == logisticAgent.Id);
            if (existingRealtion2Entity != null)
            {
                throw new Exception("This LogisticAgent is registered in MappingSorters, please delete that record first in order to update this LogisticAgent.");
            }

            entity.LogisticAgents = logisticAgent.LogisticAgents;
            await Context.SaveChangesAsync();

            return entity;
        }

        public override async Task<LogisticAgent> GetAsync(int id)
        {
            var entity = await Context.LogisticAgents.FindAsync(id);
            if (entity == null)
            {
                throw new Exception($"The logistic Agent does not exist");
            }
            return entity;
        }

        public override async Task<LogisticAgent> DeleteAsync(int id)
        {
            var entity = await Context.LogisticAgents.FindAsync(id);

            if (entity == null)
            {
                throw new Exception("The logistic Agent does not exist.");
            }
            var existingEntity = await Context.CarrierCodeLogisticAgentMappings.FirstOrDefaultAsync(x => x.LogisticAgentId == id);
            if (existingEntity != null)
            {
                throw new Exception("This LogisticAgent is mapped to a Carrier Code, please delete that record first in order to delete this LogisticAgent.");
            }
            var existingRealtion2Entity = await Context.MappingSorters.FirstOrDefaultAsync(x => x.LogisticAgentId == id);
            if (existingRealtion2Entity != null)
            {
                throw new Exception("This LogisticAgent is registered in MappingSorters, please delete that record first in order to delete this LogisticAgent.");
            }
            Context.LogisticAgents.Remove(entity);
            await Context.SaveChangesAsync();

            return entity;
        }
    }
}
