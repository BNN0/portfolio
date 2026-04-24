using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodeDivertLaneMapping;
using LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodes;
using LuxotticaSorting.ApplicationServices.Shared.DTO.Containers;
using LuxotticaSorting.ApplicationServices.Shared.DTO.PrintLabel;
using LuxotticaSorting.Core.CarrierCodes;
using LuxotticaSorting.Core.Container;
using LuxotticaSorting.Core.DivertLanes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.DataAccess.Repositories.Containers
{
    public class ContainersRepository : Repository<int, ContainerTable>
    {
        public ContainersRepository(SortingContext context) : base(context)
        {

        }
        public async Task<ContainerTable> AddAsync(ContainerTable containerAddDTO)
        {
            var existingEntity = await Context.Containers.FirstOrDefaultAsync(x => x.ContainerId == containerAddDTO.ContainerId);
            if (existingEntity != null)
            {
                throw new Exception("This Container value already exists.");
            }
            if (!((containerAddDTO.ContainerId.StartsWith("GLDTW") && containerAddDTO.ContainerId.Length == 15) || (containerAddDTO.ContainerId.StartsWith("1")&& containerAddDTO.ContainerId.Length ==10)))
            {
                throw new Exception("The Container does not comply with the nomenclature.");
            } 
            var containerType = await Context.ContainerTypes.FindAsync(containerAddDTO.ContainerTypeId);
            
            if (containerType == null)
            {
                throw new Exception("The Container Type does not exist in the database.");
            }
            containerAddDTO.Status = true;
            containerAddDTO.ContainerType = null;
            
            await Context.Containers.AddAsync(containerAddDTO);
            containerType.Containers.Add(containerAddDTO);
            await Context.SaveChangesAsync();
            
            return containerAddDTO;
        }

        public async Task<ContainerTable> AddAsyncGayLord(ContainerTable containerAddDTO)
        {
            var lastRecord = await Context.Containers
            .OrderByDescending(c => c.Id)
            .FirstOrDefaultAsync();
            var secuence = 0;
            if (lastRecord == null)
            {
                secuence = 1;
            }
            else
            {
                secuence = lastRecord.Id + 1;
            }
            string format = "GLDTW" + secuence.ToString().PadLeft(10, '0');
            containerAddDTO.ContainerId = format;
            var existingEntity = await Context.Containers.FirstOrDefaultAsync(x => x.ContainerId == containerAddDTO.ContainerId);
            if (existingEntity != null)
            {
                secuence += 1;
                format = "GLDTW" + secuence.ToString().PadLeft(10, '0');
                containerAddDTO.ContainerId = format;
                existingEntity = await Context.Containers.FirstOrDefaultAsync(x => x.ContainerId == containerAddDTO.ContainerId);
                if (existingEntity != null)
                {
                    secuence += 1;
                    format = "GLDTW" + secuence.ToString().PadLeft(10, '0');
                    containerAddDTO.ContainerId = format;
                    existingEntity = await Context.Containers.FirstOrDefaultAsync(x => x.ContainerId == containerAddDTO.ContainerId);
                    if (existingEntity != null)
                    {
                        throw new Exception("This Container value already exists.");
                    }
                }
            }
            var containerType = await Context.ContainerTypes.FindAsync(containerAddDTO.ContainerTypeId);

            if (containerType == null)
            {
                throw new Exception("The Container Type does not exist in the database.");
            }

            containerAddDTO.ContainerType = null;
            containerAddDTO.Status = true;
            await Context.Containers.AddAsync(containerAddDTO);
            containerType.Containers.Add(containerAddDTO);
            await Context.SaveChangesAsync();

            return containerAddDTO;
        }

        public async Task<ContainerTable> AddAsyncTruck(ContainerTable containerAddDTO)
        {
            var secuence = 0;
            var lastRecord = await Context.Containers
            .OrderByDescending(c => c.Id)
            .FirstOrDefaultAsync();
            if(lastRecord == null) {
                 secuence = 1;
            }
            else
            {
                secuence = lastRecord.Id + 1;
            }

            string format = "1" + secuence.ToString("D09");
            containerAddDTO.ContainerId = format;
            var existingEntity = await Context.Containers.FirstOrDefaultAsync(x => x.ContainerId == containerAddDTO.ContainerId);
            if (existingEntity != null)
            {
                secuence += 1;
                format = "1" + secuence.ToString("D09");
                containerAddDTO.ContainerId = format;
                existingEntity = await Context.Containers.FirstOrDefaultAsync(x => x.ContainerId == containerAddDTO.ContainerId);
                if (existingEntity != null)
                {
                    secuence += 1;
                    format = "1" + secuence.ToString("D09");
                    containerAddDTO.ContainerId = format;
                    existingEntity = await Context.Containers.FirstOrDefaultAsync(x => x.ContainerId == containerAddDTO.ContainerId);
                    if (existingEntity != null)
                    {
                        throw new Exception("This Container value already exists.");
                    }
                }
            }
            var containerType = await Context.ContainerTypes.FindAsync(containerAddDTO.ContainerTypeId);

            if (containerType == null)
            {
                throw new Exception("The Container Type does not exist in the database.");
            }

            containerAddDTO.ContainerType = null;
            containerAddDTO.Status = true;
            await Context.Containers.AddAsync(containerAddDTO);
            containerType.Containers.Add(containerAddDTO);
            await Context.SaveChangesAsync();
            return containerAddDTO;
        }

        public async Task<List<ContainerToShow>> GetTrucks()
        {
            var combinedDataList = (from divertlane in Context.DivertLanes
                                    join containers in Context.Containers on divertlane.ContainerId equals containers.Id where containers.ContainerId.StartsWith("1") && containers.ContainerId.Length == 10
                                    select new ContainerToShow
                                    {
                                        Id = containers.Id,
                                        ContainerId = containers.ContainerId,
                                        ContainerTypeId = containers.ContainerTypeId,
                                        DivertLaneId = divertlane.Id,
                                        DivertLaneValue = divertlane.DivertLanes,
                                        Status = containers.Status,
                                    }).ToList();

            if (!combinedDataList.Any())
            {
                return null;
            }
            return combinedDataList;
        }

        public async Task<List<ContainerToShow>> GetGaylord()
        {
            var combinedDataList = (from divertlane in Context.DivertLanes
                                    join containers in Context.Containers on divertlane.ContainerId equals containers.Id
                                    where containers.ContainerId.StartsWith("GLDTW") && containers.ContainerId.Length == 15
                                    select new ContainerToShow
                                    {
                                        Id = containers.Id,
                                        ContainerId = containers.ContainerId,
                                        ContainerTypeId = containers.ContainerTypeId,
                                        DivertLaneId = divertlane.Id,
                                        DivertLaneValue = divertlane.DivertLanes,
                                        Status = containers.Status
                                    }).ToList();
            if (!combinedDataList.Any())
            {
                return null;
            }
            return combinedDataList;
        }

        public async Task<ContainerTable> UpdateAsync(ContainerTable container)
        {
            var containerType = await Context.ContainerTypes.FindAsync(container.ContainerType.Id);
            if (containerType == null)
            {
                throw new Exception("The Container Type does not exist in the database.");
            }
            if (!((container.ContainerId.StartsWith("GLDTW") && container.ContainerId.Length == 15) || (container.ContainerId.StartsWith("1") && container.ContainerId.Length == 10)))
            {
                throw new Exception("The Container does not comply with the nomenclature.");
            }
            var entity = await Context.Containers.FindAsync(container.Id);
            if (entity == null)
            {
                throw new Exception("The Container with the specified Id does not exist.");
            }
            if (container.ContainerId != entity.ContainerId)
            {
                var existingEntity = await Context.Containers.FirstOrDefaultAsync(x => x.ContainerId == container.ContainerId);
                if (existingEntity != null)
                {
                    throw new Exception("This Container value already exists.");
                }
            }

            entity.ContainerId = container.ContainerId;
            entity.ContainerType = containerType;

            await Context.SaveChangesAsync();

            return entity;
        }

        public override async Task<ContainerTable> DeleteAsync(int id)
        {
            var entity = await Context.Containers.FindAsync(id);

            if (entity == null)
            {
                throw new Exception("The Divert Lane with the specified Id does not exist.");
            }

            Context.Containers.Remove(entity);
            await Context.SaveChangesAsync();

            return entity;
        }

        public async Task<bool> ContainerToPrint(ContainerToPrint containerToPrint)
        {
            var entityToAdd = new ContainerTable();
            var existingDivertLane = Context.DivertLanes.FirstOrDefault(x => x.Id == containerToPrint.DivertLanesId);
            if(existingDivertLane == null) {

                throw new Exception("This DivertLane does not exist");

            }
            var existingContainerInitial = await Context.Containers.FirstOrDefaultAsync(x => x.ContainerId == containerToPrint.ContainerId);
            if (existingContainerInitial != null)
            {

                throw new Exception("This Container already exists");
            }
            if (containerToPrint.ContainerId.StartsWith("GLDTW") && containerToPrint.ContainerId.Length == 15)
            {

                var existingEntity = await Context.ContainerTypes.FirstOrDefaultAsync(x => x.ContainerTypes.StartsWith("G"));

                if (existingEntity != null)
                {
                    entityToAdd = new ContainerTable
                    {
                        ContainerId = containerToPrint.ContainerId,
                        ContainerTypeId = existingEntity.Id,
                        ContainerType = null,
                        Status = true,

                    };

                    await Context.Containers.AddAsync(entityToAdd);
                    await Context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("No ContainerType for Gaylord exists.");
                }
            }

            if (containerToPrint.ContainerId.StartsWith("1") && containerToPrint.ContainerId.Length == 10)
            {
                var existingEntity = await Context.ContainerTypes.FirstOrDefaultAsync(x => x.ContainerTypes.StartsWith("T"));

                if (existingEntity != null)
                {
                    entityToAdd = new ContainerTable
                    {
                        ContainerId = containerToPrint.ContainerId,
                        ContainerTypeId = existingEntity.Id,
                        Status = true,
                        ContainerType = null
                    };

                    await Context.Containers.AddAsync(entityToAdd);
                    await Context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("No ContainerType for Truck exists.");
                }
            }
            if((!(containerToPrint.ContainerId.StartsWith("1") && containerToPrint.ContainerId.Length == 10)) && (!(containerToPrint.ContainerId.StartsWith("GLDTW") && containerToPrint.ContainerId.Length == 15)))
            {
                throw new Exception("The Container ID does not comply with the nomenclature.");
            }
            var existingContainer = await Context.Containers.FirstOrDefaultAsync(x => x.ContainerId == containerToPrint.ContainerId);
            if (existingContainer == null)
            {

                throw new Exception("The container was not registered try again");

            }
            await updateDivertLane(containerToPrint.DivertLanesId, existingContainer.Id);

            return true;
        }

        private async Task updateDivertLane(int id, int containerId)
        {
            var entity = await Context.DivertLanes.FindAsync(id);
            entity.ContainerId = containerId;
            await Context.SaveChangesAsync();
        }
    }
}
