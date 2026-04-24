using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.Shared.Dto.LogisticAgents;
using LuxotticaSorting.ApplicationServices.Shared.DTO.BoxTypes;
using LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodes;
using LuxotticaSorting.Core.BoxTypes;
using LuxotticaSorting.Core.CarrierCodes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.DataAccess.Repositories.CarrierCodes
{
    public class CarrierCodeRepository : Repository<int, CarrierCode>
    {
        public CarrierCodeRepository(SortingContext context) : base(context)
        {

        }

        public async Task<CarrierCode> AddAsync(CarrierCodeAddDto carrierCodeAddDto)
        {
            var existingEntity = await Context.CarrierCodes.FirstOrDefaultAsync(x => x.CarrierCodes == carrierCodeAddDto.CarrierCodes);
            if (existingEntity != null)
            {
                throw new Exception("The value already exists.");
            }
            var carriercode = new CarrierCode { 
                CarrierCodes = carrierCodeAddDto.CarrierCodes
            };
            await Context.CarrierCodes.AddAsync(carriercode);
            await Context.SaveChangesAsync();
            return carriercode;
        }

        public async Task<CarrierCode> UpdateAsync(int id, CarrierCodeAddDto carrierCodeAddDto)
        {
            var existingEntity = await Context.CarrierCodes.FirstOrDefaultAsync(x => x.CarrierCodes == carrierCodeAddDto.CarrierCodes);
            if (existingEntity != null)
            {
                throw new Exception("The value already exists.");
            }
            var entity = await Context.CarrierCodes.FindAsync(id);
            if (entity == null)
            {
                throw new Exception("This Carrier Code does not exist");
            }
            var existingEntity2 = await Context.CarrierCodeDivertLaneMappings.FirstOrDefaultAsync(x => x.CarrierCodeId == id);
            if (existingEntity2 != null)
            {
                throw new Exception("This Carrier Code is mapped to a divert lane, please delete that record first in order to edit this Carrier Code.");
            }
            var existingEntity1 = await Context.CarrierCodeLogisticAgentMappings.FirstOrDefaultAsync(x => x.CarrierCodeId == id);
            if (existingEntity1 != null)
            {
                throw new Exception("This Carrier Code is mapped to a logistic agent, please delete that record first in order to edit this Carrier Code.");
            }
            entity.CarrierCodes = carrierCodeAddDto.CarrierCodes;
            await Context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await Context.CarrierCodes.FindAsync(id);
            if (entity == null)
            {
                throw new Exception("This Carrier Code does not exist");
            }
            var existingEntity = await Context.CarrierCodeDivertLaneMappings.FirstOrDefaultAsync(x => x.CarrierCodeId == id);
            if (existingEntity != null)
            {
                throw new Exception("This Carrier Code is mapped to a divert lane, please delete that record first in order to delete this Carrier Code.");
            }
            var existingEntity1 = await Context.CarrierCodeLogisticAgentMappings.FirstOrDefaultAsync(x => x.CarrierCodeId == id);
            if (existingEntity1 != null)
            {
                throw new Exception("This Carrier Code is mapped to a logistic agent, please delete that record first in order to delete this Carrier Code.");
            }
            Context.Remove(entity);
            Context.SaveChanges();
        }
    }
}
