using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.Core.ContainerTypes;
using Microsoft.EntityFrameworkCore;

namespace LuxotticaSorting.DataAccess.Repositories.ContainerTypes
{
    public class ContainerTypeRepository : Repository<int, ContainerType>//replace real entity DB
    {
        public ContainerTypeRepository(SortingContext context) : base(context)
        {
            
        }

        public override async Task<ContainerType> AddAsync(ContainerType containerType)
        {
            var entity = await Context.ContainerTypes.FirstOrDefaultAsync(x => x.ContainerTypes == containerType.ContainerTypes);
            if (entity != null)
            {
                throw new InvalidOperationException("The Container type value already exists in the database.");
            }

            await Context.ContainerTypes.AddAsync(containerType);
            await Context.SaveChangesAsync();

            return containerType;
        }

        public override async Task<ContainerType> UpdateAsync(ContainerType containerType)
        {
            var entity = await Context.ContainerTypes.FindAsync(containerType.Id);
            if (entity == null)
            {
                throw new Exception("The container type with the specified Id does not exist.");
            }

            var existingEntity = await Context.ContainerTypes.FirstOrDefaultAsync(x => x.ContainerTypes == containerType.ContainerTypes);
            if (existingEntity != null && entity.ContainerTypes != containerType.ContainerTypes)
            {
                throw new Exception("The Container type value already exists.");
            }
            var existingRelation1Entity = await Context.Containers.FirstOrDefaultAsync(x => x.ContainerTypeId == containerType.Id);
            if (existingRelation1Entity != null)
            {
                throw new Exception("This Container Type is related to Containers. First delete that record to update this Container Type.");
            }
            var existingRelation2Entity = await Context.MappingSorters.FirstOrDefaultAsync(x => x.ContainerTypeId == containerType.Id);
            if (existingRelation2Entity != null)
            {
                throw new Exception("This Container Type is related in Mapping Sorters. First unregister to update this ContainerType.");
            }
            entity.ContainerTypes = containerType.ContainerTypes;
            await Context.SaveChangesAsync();

            return entity;
        }

        public override async Task<ContainerType> GetAsync(int id)
        {
            var entity =  await Context.ContainerTypes.FindAsync(id); 
            if (entity == null) 
            {
                throw new Exception($"The container type does not exist");
            }
            return entity;
        }

        public override async Task<ContainerType> DeleteAsync(int id)
        {
            var entity = await Context.ContainerTypes.FindAsync(id);

            if (entity == null)
            {
                throw new Exception("The container type with the specified Id does not exist.");
            }

            var existingRelation1Entity = await Context.Containers.FirstOrDefaultAsync(x => x.ContainerTypeId == id);
            if (existingRelation1Entity != null)
            {
                throw new Exception("This Container Type is related to Containers. First delete that record to update this Container Type.");
            }
            var existingRelation2Entity = await Context.MappingSorters.FirstOrDefaultAsync(x => x.ContainerTypeId == id);
            if (existingRelation2Entity != null)
            {
                throw new Exception("This Container Type is related in Mapping Sorters. First unregister to update this ContainerType.");
            }

            Context.ContainerTypes.Remove(entity);
            await Context.SaveChangesAsync();

            return entity;
        }
    }
}
