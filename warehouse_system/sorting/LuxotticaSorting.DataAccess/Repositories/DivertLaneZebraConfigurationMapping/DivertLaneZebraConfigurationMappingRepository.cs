using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.Shared.DTO.DivertLaneZebraConfigurationMapping;
using LuxotticaSorting.ApplicationServices.Shared.DTO.ZebraHistorial;
using LuxotticaSorting.Core.Container;
using LuxotticaSorting.Core.DivertLanes;
using LuxotticaSorting.Core.Mapping.CarrierCodeDivertLine;
using LuxotticaSorting.Core.Mapping.DivertLaneZebraConfiguration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.DataAccess.Repositories.DivertLaneZebraConfigurationMapping
{
    public class DivertLaneZebraConfigurationMappingRepository : Repository<int, LuxotticaSorting.Core.Mapping.DivertLaneZebraConfiguration.DivertLaneZebraConfigurationMapping>
    {
        public DivertLaneZebraConfigurationMappingRepository(SortingContext context) : base(context)
        {

        }
        public override async Task<Core.Mapping.DivertLaneZebraConfiguration.DivertLaneZebraConfigurationMapping> AddAsync(Core.Mapping.DivertLaneZebraConfiguration.DivertLaneZebraConfigurationMapping divertLane)
        {
            var existingDivertLaneMapping = await Context.DivertLaneZebraConfigurationMappings
        .FirstOrDefaultAsync(x => x.DivertLaneId == divertLane.DivertLaneId);

            if (existingDivertLaneMapping != null)
            {
                throw new InvalidOperationException("A Divert Lane can have only one printer configuration.");
            }
            var existingDivertLane = await Context.DivertLanes
        .FirstOrDefaultAsync(x => x.Id == divertLane.DivertLaneId);
            if (existingDivertLane == null)
            {
                throw new InvalidOperationException("This Divert Lane is not registered.");
            }

            var existingZebra = await Context.ZebraConfigurations
        .FirstOrDefaultAsync(x => x.Id == divertLane.ZebraConfigurationId);
            if (existingZebra == null)
            {
                throw new InvalidOperationException("This Zebra Configuration is not registered.");
            }
            await Context.DivertLaneZebraConfigurationMappings.AddAsync(divertLane);
            await Context.SaveChangesAsync();

            return divertLane;
        }

        public override async Task<Core.Mapping.DivertLaneZebraConfiguration.DivertLaneZebraConfigurationMapping> UpdateAsync(Core.Mapping.DivertLaneZebraConfiguration.DivertLaneZebraConfigurationMapping divertLaneZebraConfigurationMapping)
        {
            var entity = await Context.DivertLaneZebraConfigurationMappings.FirstOrDefaultAsync(x => x.Id == divertLaneZebraConfigurationMapping.Id);

            if (entity == null)
            {
                throw new Exception("Mapping from divert Lane to Zebra Configuration does not exist.");
            }

            var existingDivertLane = await Context.DivertLanes
            .FirstOrDefaultAsync(x => x.Id == divertLaneZebraConfigurationMapping.DivertLaneId);
            if (existingDivertLane == null)
            {
                throw new InvalidOperationException("This Divert Lane is not registered.");
            }

            if (divertLaneZebraConfigurationMapping.DivertLaneId != entity.DivertLaneId)
            {
                var existingDivertLaneMapping = await Context.DivertLaneZebraConfigurationMappings
                .FirstOrDefaultAsync(x => x.DivertLaneId == divertLaneZebraConfigurationMapping.DivertLaneId);

                if (existingDivertLaneMapping != null)
                {
                    throw new InvalidOperationException("A Divert Lane can have only one printer configuration.");
                }
            }


            var existingZebra = await Context.ZebraConfigurations
            .FirstOrDefaultAsync(x => x.Id == divertLaneZebraConfigurationMapping.ZebraConfigurationId);
            if (existingZebra == null)
            {
                throw new InvalidOperationException("This Zebra Configuration is not registered.");
            }
            entity.DivertLaneId = divertLaneZebraConfigurationMapping.DivertLaneId;
            entity.ZebraConfigurationId = divertLaneZebraConfigurationMapping.ZebraConfigurationId;

            await Context.SaveChangesAsync();

            return entity;
        }

        public override async Task<Core.Mapping.DivertLaneZebraConfiguration.DivertLaneZebraConfigurationMapping> DeleteAsync(int id)
        {
            var entity = await Context.DivertLaneZebraConfigurationMappings.FindAsync(id);

            if (entity == null)
            {
                throw new Exception("Mapping from divert Lane to Zebra Configuration does not exist.");
            }

            Context.DivertLaneZebraConfigurationMappings.Remove(entity);
            await Context.SaveChangesAsync();

            return entity;
        }

        public async Task<List<DivertLanesZebraConfigurationCombinated>> GetCombinatedData()
        {

            var entitiesToShow = (from mapping in Context.DivertLaneZebraConfigurationMappings
                                  join divertlanes in Context.DivertLanes on mapping.DivertLaneId equals divertlanes.Id
                                  join printer in Context.ZebraConfigurations on mapping.ZebraConfigurationId equals printer.Id
                                  select new DivertLanesZebraConfigurationCombinated
                                  {
                                      Id = mapping.Id,
                                      DivertLaneId = mapping.DivertLaneId,
                                      DivertLaneValue = divertlanes.DivertLanes,
                                      ZebraConfigurationId = mapping.ZebraConfigurationId,
                                      ZebraConfigurationName = printer.NamePrinter,
                                  }).ToList();

            if (entitiesToShow == null)
            {
                throw new Exception($"No records of mappings from divert lane to zebra configuration");
            }

            return entitiesToShow;
        }
    }
}
