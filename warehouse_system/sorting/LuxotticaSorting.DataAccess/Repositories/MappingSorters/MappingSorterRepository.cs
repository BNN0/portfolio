using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.Shared.Dto.MappingSorters;
using LuxotticaSorting.ApplicationServices.Shared.DTO.MappingSorters;
using LuxotticaSorting.Core.BoxTypes;
using LuxotticaSorting.Core.CarrierCodes;
using LuxotticaSorting.Core.MappingSorter;
using Microsoft.EntityFrameworkCore;

namespace LuxotticaSorting.DataAccess.Repositories.MappingSorters
{
    public class MappingSorterRepository : Repository<int, MappingSorter>
    {
        public MappingSorterRepository(SortingContext context) : base(context)
        {
        }

        public override async Task<MappingSorter> AddAsync(MappingSorter mappingSorter)
        {
            var existingDivertLanes = await Context.DivertLanes.ToListAsync();

            foreach (var divertLane in existingDivertLanes)
            {
                var existingMapping = await Context.MappingSorters
                    .FirstOrDefaultAsync(ms => ms.DivertLaneId == divertLane.Id);

                if (existingMapping != null)
                {
                    continue;
                }

                int? carrierCodeId = null;

                var carrierCodeMapping = await Context.CarrierCodeDivertLaneMappings
                    .Where(ccdlm => ccdlm.DivertLaneId == divertLane.Id && ccdlm.Status)
                    .FirstOrDefaultAsync();

                if (carrierCodeMapping != null)
                {
                    carrierCodeId = carrierCodeMapping.CarrierCodeId;
                }

                var mappingSorterItem = new MappingSorter
                {
                    DivertLaneId = divertLane.Id,

                    BoxTypeId = string.Join(",", await Context.BoxTypeDivertLaneMappings
                        .Where(btdl => btdl.DivertLaneId == divertLane.Id)
                        .Select(btdl => btdl.BoxTypeId.ToString())
                        .ToListAsync()),

                    CarrierCodeId = string.Join(",", await Context.CarrierCodeDivertLaneMappings
                        .Where(btdl => btdl.DivertLaneId == divertLane.Id)
                        .Select(btdl => btdl.CarrierCodeId.ToString())
                        .ToListAsync()),

                    LogisticAgentId = carrierCodeId != null
                        ? (await Context.CarrierCodeLogisticAgentMappings
                            .Where(cclam => cclam.CarrierCodeId == carrierCodeId)
                            .Select(cclam => cclam.LogisticAgentId)
                            .FirstOrDefaultAsync()) : null
                };

                await Context.MappingSorters.AddAsync(mappingSorterItem);
            }

            await Context.SaveChangesAsync();

            return mappingSorter;
        }

        public override async Task<MappingSorter> UpdateAsync(MappingSorter mappingSorter)
        {
            var entity = await Context.MappingSorters
                .FirstOrDefaultAsync(x => x.Id == mappingSorter.Id);

            if (entity == null)
            {
                throw new Exception("The Sorter Mapping does not exist.");
            }

            var auxAgent = await Context.LogisticAgents.FindAsync(mappingSorter.LogisticAgentId);
            if (auxAgent != null)
            {
                entity.LogisticAgent = auxAgent;
            }

            entity.CarrierCodeId = mappingSorter.CarrierCodeId;

            entity.BoxTypeId = mappingSorter.BoxTypeId;

            await Context.SaveChangesAsync();

            return entity;
        }

        public async Task<List<MappingSorterGetAllDto>> GetCombinedDataAsync()
        {
            var combinedDataList = await (
                from mapping in Context.MappingSorters
                join divertLane in Context.DivertLanes on mapping.DivertLaneId equals divertLane.Id
                join logisticAgent in Context.LogisticAgents on mapping.LogisticAgentId equals logisticAgent.Id into logisticAgentsGroup
                from logisticAgent in logisticAgentsGroup.DefaultIfEmpty()
                join carrierCode in Context.CarrierCodes on mapping.Id equals carrierCode.Id into carrierCodesGroup
                from carrierCode in carrierCodesGroup.DefaultIfEmpty()
                join containerType in Context.ContainerTypes on mapping.ContainerTypeId equals containerType.Id into containerTypesGroup
                from containerType in containerTypesGroup.DefaultIfEmpty()
                join boxType in Context.BoxTypes on mapping.Id equals boxType.Id into boxTypeGroup
                from boxType in boxTypeGroup.DefaultIfEmpty()
                select new
                {
                    mapping.Id,
                    mapping.DivertLaneId,
                    divertLane.DivertLanes,
                    divertLane.Status,
                    mapping.LogisticAgentId,
                    LogisticAgents = logisticAgent != null ? logisticAgent.LogisticAgents : null,
                    mapping.CarrierCodeId,
                    CarrierCodes = carrierCode != null ? carrierCode.CarrierCodes : null,
                    mapping.ContainerTypeId,
                    ContainerTypes = containerType != null ? containerType.ContainerTypes : null,
                    mapping.BoxTypeId
                }
            ).ToListAsync();

            var result = combinedDataList.Select(data => new MappingSorterGetAllDto
            {
                Id = data.Id,
                DivertLaneId = data.DivertLaneId,
                DivertLanes = data.DivertLanes,
                Status = data.Status,
                LogisticAgentId = data.LogisticAgentId != null ? (int)data.LogisticAgentId : 0,
                LogisticAgents = data.LogisticAgents,
                CarrierCodes = data.CarrierCodeId != null
                    ? data.CarrierCodeId.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(btId => int.Parse(btId))
                        .Distinct()
                        .Select(btId => new CarrierCode
                        {
                            Id = btId,
                            CarrierCodes = Context.CarrierCodes
                                .Where(bt => bt.Id == btId)
                                .Select(bt => bt.CarrierCodes)
                                .FirstOrDefault()
                        })
                        .ToList()
                    : new List<CarrierCode>(),
                ContainerTypeId = data.ContainerTypeId != null ? (int)data.ContainerTypeId : 0,
                ContainerTypes = data.ContainerTypes,
                BoxTypes = data.BoxTypeId != null
                    ? data.BoxTypeId.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(btId => int.Parse(btId))
                        .Distinct()
                        .Select(btId => new BoxType
                        {
                            Id = btId,
                            BoxTypes = Context.BoxTypes
                                .Where(bt => bt.Id == btId)
                                .Select(bt => bt.BoxTypes)
                                .FirstOrDefault()
                        })
                        .ToList()
                    : new List<BoxType>()
            }).ToList();

            return result;
        }



        public async Task<MappingSorterAddDivertContainerTyDto> AddMappingDivertLaneAndContainerTypeT(MappingSorterAddDivertContainerTyDto mapingDivertLaneContianeType)
        {
            var existingDivertLane = await Context.MappingSorters
                 .FirstOrDefaultAsync(x => x.DivertLaneId == mapingDivertLaneContianeType.DivertLaneId);

            if (existingDivertLane != null)
            {
                if (existingDivertLane.ContainerTypeId == 0 || existingDivertLane.ContainerTypeId == null)
                {
                    var searchTypeT = await (from ct in Context.ContainerTypes
                                             where ct.Id == mapingDivertLaneContianeType.ContainerTypeId
                                             select ct.ContainerTypes).FirstOrDefaultAsync();

                    if (searchTypeT != null)
                    {
                        if (searchTypeT == "T")
                        {
                            existingDivertLane.ContainerTypeId = mapingDivertLaneContianeType.ContainerTypeId;
                            Context.MappingSorters.Update(existingDivertLane);
                            await Context.SaveChangesAsync();

                            return mapingDivertLaneContianeType;
                        }
                        else
                        {
                            throw new InvalidOperationException("The Container Type Invalid for Truck!.");
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("The Container Type does not exist!.");
                    }
                }
                else
                {
                    throw new InvalidOperationException("The Divert Lane already registered.");
                }
            }

            if (mapingDivertLaneContianeType.ContainerTypeId != 0)
            {
                var searchTypeT = await (from ct in Context.ContainerTypes
                                         where ct.Id == mapingDivertLaneContianeType.ContainerTypeId
                                         select ct.ContainerTypes).FirstOrDefaultAsync();

                if (searchTypeT != null)
                {
                    if (searchTypeT == "T")
                    {
                        var newMappingSorterContainerT = new MappingSorter
                        {
                            DivertLaneId = mapingDivertLaneContianeType.DivertLaneId,
                            ContainerTypeId = mapingDivertLaneContianeType.ContainerTypeId
                        };

                        Context.MappingSorters.AddAsync(newMappingSorterContainerT);
                        await Context.SaveChangesAsync();

                        return mapingDivertLaneContianeType;
                    }
                    else
                    {
                        throw new InvalidOperationException("The Container Type Invalid for Truck!.");
                    }
                }
                else
                {
                    throw new InvalidOperationException("The Container Type does not exist!.");
                }
            }
            var newMappingSorter = new MappingSorter
            {
                DivertLaneId = mapingDivertLaneContianeType.DivertLaneId,
                ContainerTypeId = null
            };

            await Context.MappingSorters.AddAsync(newMappingSorter);
            await Context.SaveChangesAsync();

            return mapingDivertLaneContianeType;
        }

        public async Task<MappingSorterAddDivertContainerTyDto> AddMappingDivertLaneAndContainerTypeG(MappingSorterAddDivertContainerTyDto mapingDivertLaneContianeType)
        {
            var existingDivertLane = await Context.MappingSorters
                 .FirstOrDefaultAsync(x => x.DivertLaneId == mapingDivertLaneContianeType.DivertLaneId);

            if (existingDivertLane != null)
            {
                if (existingDivertLane.ContainerTypeId == 0 || existingDivertLane.ContainerTypeId == null)
                {
                    var searchTypeT = await (from ct in Context.ContainerTypes
                                             where ct.Id == mapingDivertLaneContianeType.ContainerTypeId
                                             select ct.ContainerTypes).FirstOrDefaultAsync();

                    if (searchTypeT != null)
                    {
                        if (searchTypeT == "G")
                        {
                            existingDivertLane.ContainerTypeId = mapingDivertLaneContianeType.ContainerTypeId;
                            Context.MappingSorters.Update(existingDivertLane);
                            await Context.SaveChangesAsync();

                            return mapingDivertLaneContianeType;
                        }
                        else
                        {
                            throw new InvalidOperationException("The Container Type Invalid for Gaylord!.");
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("The Container Type does not exist!.");
                    }
                }
                else
                {
                    throw new InvalidOperationException("The Divert Lane already registered.");
                }
            }

            if (mapingDivertLaneContianeType.ContainerTypeId != 0)
            {
                var searchTypeT = await (from ct in Context.ContainerTypes
                                         where ct.Id == mapingDivertLaneContianeType.ContainerTypeId
                                         select ct.ContainerTypes).FirstOrDefaultAsync();

                if (searchTypeT != null)
                {
                    if (searchTypeT == "G")
                    {
                        var newMappingSorterContainerT = new MappingSorter
                        {
                            DivertLaneId = mapingDivertLaneContianeType.DivertLaneId,
                            ContainerTypeId = mapingDivertLaneContianeType.ContainerTypeId
                        };

                        await Context.MappingSorters.AddAsync(newMappingSorterContainerT);
                        await Context.SaveChangesAsync();

                        return mapingDivertLaneContianeType;
                    }
                    else
                    {
                        throw new InvalidOperationException("The Container Type Invalid for Gaylord!.");
                    }
                }
                else
                {
                    throw new InvalidOperationException("The Container Type does not exist!.");
                }
            }
            var newMappingSorter = new MappingSorter
            {
                DivertLaneId = mapingDivertLaneContianeType.DivertLaneId,
                ContainerTypeId = null
            };

            await Context.MappingSorters.AddAsync(newMappingSorter);
            await Context.SaveChangesAsync();

            return mapingDivertLaneContianeType;
        }

        public async Task<MappingSorter> DeleteValueColumnContainerType(int id)
        {
            var entity = await Context.MappingSorters.FindAsync(id);

            if (entity == null)
            {
                throw new InvalidOperationException("The sorter mapping does not exist.");
            }

            if (entity.ContainerTypeId == null || entity.ContainerTypeId == 0)
            {
                throw new InvalidOperationException("The container type has already been removed previously.");
            }

            entity.ContainerTypeId = null;
            Context.MappingSorters.Update(entity);
            await Context.SaveChangesAsync();
            return entity;
        }

        public async Task<List<MappingSorterFrontendViewDto>> GetOnlyDivertLaneContainerType()
        {
            var searchOnlyDivertAndContainerData = (from mapping in Context.MappingSorters
                                                    join divertLane in Context.DivertLanes on mapping.DivertLaneId equals divertLane.Id
                                                    select new MappingSorterFrontendViewDto
                                                    {
                                                        Id = mapping.Id,
                                                        DivertLaneId = mapping.DivertLaneId,
                                                        DivertLanes = divertLane.DivertLanes,
                                                        ContainerTypeId = (int)mapping.ContainerTypeId,
                                                        ContainerTypes = mapping.ContainerType.ContainerTypes
                                                    }).ToList();

            if (searchOnlyDivertAndContainerData == null)
            {
                return null;
            }
            return searchOnlyDivertAndContainerData;
        }
    }
}

