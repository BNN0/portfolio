using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.Shared.DTO.BoxTypes;
using LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodes;
using LuxotticaSorting.ApplicationServices.Shared.DTO.Containers;
using LuxotticaSorting.Core.BoxTypes;
using LuxotticaSorting.Core.CarrierCodes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.DataAccess.Repositories.BoxTypes
{
    public class BoxTypeRepository : Repository<int, BoxType>
    {
        public BoxTypeRepository(SortingContext context) : base(context)
        {

        }

        public async Task<BoxType> AddAsync(BoxTypesAddDTO boxTypesAddDTO)
        {
            var existingEntity = await Context.BoxTypes.FirstOrDefaultAsync(x => x.BoxTypes == boxTypesAddDTO.BoxTypes);
            if (existingEntity != null)
            {
                throw new Exception("This BoxType value already exists.");
            }
            var boxType = new BoxType {
            BoxTypes = boxTypesAddDTO.BoxTypes
            };

            await Context.BoxTypes.AddAsync(boxType);
            await Context.SaveChangesAsync();
            return boxType;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await Context.BoxTypes.FindAsync(id);
            if (entity == null)
            {
                throw new Exception("This BoxType does not exist");
            }
            var existingEntity = await Context.BoxTypeDivertLaneMappings.FirstOrDefaultAsync(x => x.BoxTypeId == id);
            if (existingEntity != null)
            {
                throw new Exception("This BoxType is mapped to a divert line, please delete that record first in order to delete this BoxType.");
            }
            Context.Remove(entity);
            Context.SaveChanges();
        }

        public async Task<BoxType> UpdateAsync(int id, BoxTypesAddDTO boxTypesAddDTO)
        {
            var entity = await Context.BoxTypes.FindAsync(id);
            if (entity == null)
            {
                throw new Exception("This BoxType does not exist");
            }
            var existingEntity = await Context.BoxTypes.FirstOrDefaultAsync(x => x.BoxTypes == boxTypesAddDTO.BoxTypes);
            if (existingEntity != null)
            {
                throw new Exception("This BoxType value already exists.");
            }
            var existingEntity2 = await Context.BoxTypeDivertLaneMappings.FirstOrDefaultAsync(x => x.BoxTypeId == id);
            if (existingEntity2 != null)
            {
                throw new Exception("This BoxType is mapped to a divert line, please delete that record first in order to delete this BoxType.");
            }
            entity.BoxTypes = boxTypesAddDTO.BoxTypes;
            await Context.SaveChangesAsync();
            return entity;
        }
    }
}
