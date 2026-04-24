﻿using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.Shared.Dto.ContainerTypes;
using LuxotticaSorting.ApplicationServices.Shared.DTO.BoxTypes;
using LuxotticaSorting.ApplicationServices.Shared.DTO.ConfirmationBoxes;
using LuxotticaSorting.ApplicationServices.Shared.DTO.Containers;
using LuxotticaSorting.ApplicationServices.Shared.DTO.DivertLanes;
using LuxotticaSorting.ApplicationServices.Shared.DTO.LaneStatus;
using LuxotticaSorting.ApplicationServices.Shared.DTO.PrintLabel;
using LuxotticaSorting.Core.BoxTypes;
using LuxotticaSorting.Core.Container;
using LuxotticaSorting.Core.ContainerTypes;
using LuxotticaSorting.Core.DivertLanes;
using LuxotticaSorting.Core.Mapping.CarrierCodeDivertLine;
using LuxotticaSorting.DataAccess.Repositories.ConfirmationBoxes;
using LuxotticaSorting.DataAccess.Repositories.Containers;
using LuxotticaSorting.DataAccess.Repositories.PrintLabels;
using LuxotticaSorting.DataAccess.Repositories.RoutingV10S;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LuxotticaSorting.DataAccess.Repositories.DivertLanes
{
    public class DivertLanesRepository : Repository<int, DivertLane>
    {
        private readonly ContainersRepository _containersRepository;
        private readonly PrintLabelRepository _printLabelRepository;
        private readonly ConfirmationBoxesRepository _confirmationBoxesRepository;

        public DivertLanesRepository(SortingContext context, ContainersRepository containersRepository, PrintLabelRepository printLabelRepository, ConfirmationBoxesRepository confirmationBoxesRepository) : base(context)
        {
            _containersRepository = containersRepository;
            _printLabelRepository = printLabelRepository;
            _confirmationBoxesRepository = confirmationBoxesRepository;
        }

        public override async Task<DivertLane> AddAsync(DivertLane divertLane)
        {
            var existingEntity = await Context.DivertLanes.FirstOrDefaultAsync(x => x.DivertLanes == divertLane.DivertLanes);
            if (existingEntity != null)
            {
                throw new Exception("This Divert Lane value already exists.");
            }


            await Context.DivertLanes.AddAsync(divertLane);


            await Context.SaveChangesAsync();
            return divertLane;
        }

        public async Task<DivertLane> AddAsyncCreation(DivertLanesAddCreationDTO divertLanesAddDto)
        {
            var existingEntity = await Context.DivertLanes.FirstOrDefaultAsync(x => x.DivertLanes == divertLanesAddDto.DivertLanes);
            if (existingEntity != null)
            {
                throw new Exception("This Divert Lane value already exists.");
            }
            var diverLane = new DivertLane
            {
                DivertLanes = divertLanesAddDto.DivertLanes,
                Status = divertLanesAddDto.Status,
                Full = divertLanesAddDto.Full
            };
            await Context.DivertLanes.AddAsync(diverLane);

            await Context.SaveChangesAsync();
            return diverLane;
        }

        public async Task<List<DivertLane>> GetDivertLanesTruckToPrint()
        {
            var existingEntity = await Context.ContainerTypes.FirstOrDefaultAsync(x => x.ContainerTypes.StartsWith("T"));
            if (existingEntity == null)
            {
                throw new Exception("There are no available Container Type to Truck, please add one.");
            }
            var result = Context.DivertLanes
                .Where(entity => Context.DivertLaneZebraConfigurationMappings.Any(a => a.DivertLaneId == entity.Id) &&
                        Context.MappingSorters.Any(b => b.DivertLaneId == entity.Id && b.ContainerTypeId == existingEntity.Id && b.CarrierCodeId != null && b.LogisticAgentId != null && b.CarrierCodeId.Length != 0))
                .ToList();
            if (!result.Any())
            {
                throw new Exception("There are no available Divert Lanes to print.");
            }
            return result;
        }

        public async Task<List<DivertLane>> GetDivertLanesGaylordToPrint()
        {
            var existingEntity = await Context.ContainerTypes.FirstOrDefaultAsync(x => x.ContainerTypes.StartsWith("G"));
            if (existingEntity == null)
            {
                throw new Exception("There are no available Container Type to Gaylord, please add one.");
            }
            var result = Context.DivertLanes
                .Where(entity => Context.DivertLaneZebraConfigurationMappings.Any(a => a.DivertLaneId == entity.Id) &&
                        Context.MappingSorters.Any(b => b.DivertLaneId == entity.Id && b.ContainerTypeId == existingEntity.Id && b.CarrierCodeId != null && b.LogisticAgentId != null && b.CarrierCodeId.Length != 0))
                .ToList();
            if (!result.Any())
            {
                throw new Exception("There are no available Divert Lanes to print.");
            }
            return result;
        }
        public override async Task<DivertLane> UpdateAsync(DivertLane divertLane)
        {
            var entity = await Context.DivertLanes.FindAsync(divertLane.Id);
            if (entity == null)
            {
                throw new Exception("The Divert Lane with the specified Id does not exist.");
            }
            
            var existingEntity = await Context.BoxTypeDivertLaneMappings.FirstOrDefaultAsync(x => x.DivertLaneId == divertLane.Id);
            if (existingEntity != null)
            {
                throw new Exception("This Divert Lane is mapped to a Box Type, please delete that record first in order to delete this DivertLane.");
            }
            
            var existingEntity2 = await Context.CarrierCodeDivertLaneMappings.FirstOrDefaultAsync(x => x.DivertLaneId == divertLane.Id);
            if (existingEntity2 != null)
            {
                throw new Exception("This Divert Lane is mapped to a Carrier Code, please delete that record first in order to delete this DivertLane.");
            }
            
            if (divertLane.DivertLanes != entity.DivertLanes)
            {
                var existingEntity3 = await Context.DivertLanes.FirstOrDefaultAsync(x => x.DivertLanes == divertLane.DivertLanes);
                if (existingEntity3 != null)
                {
                    throw new Exception("This Divert Lane value already exists.");
                }
            }

            var state = divertLane.Status;

            entity.Status = state;
            entity.DivertLanes = divertLane.DivertLanes;
            entity.Full = divertLane.Full;
            await Context.SaveChangesAsync();

            return entity;
        }

        public async Task<DivertLane> UpdateCreationAsync(int id, DivertLanesAddCreationDTO divertLane)
        {
            var existingEntity = await Context.BoxTypeDivertLaneMappings.FirstOrDefaultAsync(x => x.DivertLaneId == id);
            if (existingEntity != null)
            {
                throw new Exception("This Divert Lane is mapped to a Box Type, please delete that record first in order to edit this DivertLane.");
            }
            var existingEntity2 = await Context.CarrierCodeDivertLaneMappings.FirstOrDefaultAsync(x => x.DivertLaneId == id);
            if (existingEntity2 != null)
            {
                throw new Exception("This Divert Lane is mapped to a Carrier Code, please delete that record first in order to edit this DivertLane.");
            }
            var entity = await Context.DivertLanes.FindAsync(id);
            if (entity == null)
            {
                throw new Exception("The Divert Lane with the specified Id does not exist.");
            }
            if (divertLane.DivertLanes != entity.DivertLanes)
            {
                var existingEntity3 = await Context.DivertLanes.FirstOrDefaultAsync(x => x.DivertLanes == divertLane.DivertLanes);
                if (existingEntity3 != null)
                {
                    throw new Exception("This Divert Lane value already exists.");
                }
            }
            var state = divertLane.Status;

            entity.Status = state;
            entity.Full = false;
            entity.DivertLanes = divertLane.DivertLanes;
            await Context.SaveChangesAsync();

            return entity;
        }


        public override async Task<DivertLane> DeleteAsync(int id)
        {
            var entity = await Context.DivertLanes.FindAsync(id);

            if (entity == null)
            {
                throw new Exception("The Divert Lane with the specified Id does not exist.");
            }
            if (entity.Status == true)
            {
                throw new Exception("You cannot delete an active DiverLane.");
            }
            var existingEntity = await Context.BoxTypeDivertLaneMappings.FirstOrDefaultAsync(x => x.DivertLaneId == id);
            if (existingEntity != null)
            {
                throw new Exception("This Divert Lane is mapped to a Box Type, please delete that record first in order to delete this DivertLane.");
            }
            var existingEntity2 = await Context.CarrierCodeDivertLaneMappings.FirstOrDefaultAsync(x => x.DivertLaneId == id);
            if (existingEntity2 != null)
            {
                throw new Exception("This Divert Lane is mapped to a Carrier Code, please delete that record first in order to delete this DivertLane.");
            }
            var existingEntity3 = await Context.DivertLaneZebraConfigurationMappings.FirstOrDefaultAsync(x => x.DivertLaneId == id);
            if (existingEntity3 != null)
            {
                throw new Exception("This DivertLane is mapped to a Zebra Configuration, please delete that record first in order to delete this DivertLane.");
            }
            var existingSorter = await Context.MappingSorters.FirstOrDefaultAsync(x => x.DivertLaneId == id);
            if(existingSorter != null)
            {
                await deleteMappingSorter(id);
            }

            Context.DivertLanes.Remove(entity);
            await Context.SaveChangesAsync();

            return entity;
        }


        public async Task UpdateLaneStatus(LaneStatusDto laneStatusDto)
        {
            try
            {
                var properties = typeof(LaneStatusDto).GetProperties();
                var regex = new Regex(@"lane_(\d+)_(status|full)");

                foreach (var property in properties)
                {
                    if (property.PropertyType == typeof(int))
                    {
                        var propertyName = property.Name;
                        var propertyValue = (int)property.GetValue(laneStatusDto);

                        var match = regex.Match(propertyName);

                        if (match.Success && match.Groups.Count == 3 && int.TryParse(match.Groups[1].Value, out var laneNumber))
                        {
                            var divertLaneExist = await Context.DivertLanes.Where(dl => dl.DivertLanes == laneNumber).Select(dl => dl).FirstOrDefaultAsync() ?? null;

                            if (divertLaneExist != null)
                            {
                                if (propertyName.EndsWith("_status"))
                                {
                                    #region When DivertLane Turn On
                                    if(divertLaneExist.Status == false && propertyValue == 1)
                                    {
                                        var containerinDivert = await Context.Containers.FirstOrDefaultAsync(c => c.Id == divertLaneExist.ContainerId) ?? null;

                                        if(containerinDivert != null)
                                        {
                                            #region Print new Ticket

                                            if (divertLaneExist.ContainerId != null && divertLaneExist.ContainerId > -1)
                                            {
                                                PrintLabelDTO printLabel = new PrintLabelDTO
                                                {
                                                    ContainerId = (int)divertLaneExist.ContainerId,
                                                    DivertLaneId = divertLaneExist.Id
                                                };

                                                var printStatus = await _printLabelRepository.PrintLabel(printLabel);
                                            }
                                            #endregion
                                        }
                                        
                                    }
                                    #endregion

                                    else
                                    
                                    #region When DivertLane Turn Off
                                    if (divertLaneExist.Status == true && propertyValue == 0)
                                    {
                                        var containerinDivert = await Context.Containers.FirstOrDefaultAsync(c => c.Id == divertLaneExist.ContainerId) ?? null;

                                        if (containerinDivert != null)
                                        {
                                            BoxfromRoutingReqDto boxfromRoutingReqDto = new BoxfromRoutingReqDto
                                            {
                                                ContainerId = containerinDivert.ContainerId,
                                                DivertLane = divertLaneExist.DivertLanes
                                            };

                                            #region Delete incomplete waves (status null) from multiBox_Wave table
                                            await _confirmationBoxesRepository.ReasignWaves(boxfromRoutingReqDto);
                                            #endregion

                                            #region Update Qty to Registers with ContainerId and DivertLaneId correct in Border Routing
                                            await _confirmationBoxesRepository.UpdateBoxesQtyBorder(boxfromRoutingReqDto);
                                            #endregion

                                            #region MultiBoxPrint
                                            if (divertLaneExist.DivertLanes == 2 || divertLaneExist.DivertLanes == 4)
                                            {
                                                var multiBoxResult = await _printLabelRepository.PrintLabelMultiBox(boxfromRoutingReqDto);
                                            }
                                            #endregion

                                            #region Generate and Set to actual DivertLane a new ContainerId
                                            var containerTypeInContainer = await Context.ContainerTypes.FirstOrDefaultAsync(ct => ct.Id == containerinDivert.ContainerTypeId) ?? null;

                                            if (containerTypeInContainer != null && (containerTypeInContainer.ContainerTypes == "G"))
                                            {
                                                ContainerTable generateContainerId = null;
                                                ContainerType containerType = new ContainerType
                                                {
                                                    Id = containerTypeInContainer.Id,
                                                    ContainerTypes = null,
                                                    Containers = null
                                                };

                                                ContainerTable containerDto = new ContainerTable
                                                {
                                                    Id = 0,
                                                    ContainerId = "",
                                                    ContainerType = containerType,
                                                    ContainerTypeId = containerinDivert.ContainerTypeId
                                                };

                                                if (containerTypeInContainer.ContainerTypes == "G")
                                                {
                                                    generateContainerId = await _containersRepository.AddAsyncGayLord(containerDto);
                                                }

                                                if (generateContainerId != null)
                                                {
                                                    divertLaneExist.ContainerId = generateContainerId.Id;
                                                    await Context.SaveChangesAsync();
                                                }
                                            }
                                            #endregion

                                        }
                                        else
                                        {
                                            var newContainerType4Container = new ContainerType();

                                            if (divertLaneExist.DivertLanes >= 5 && divertLaneExist.DivertLanes <= 28)
                                            {
                                                newContainerType4Container = await Context.ContainerTypes.Where(ct => ct.ContainerTypes == "G").FirstOrDefaultAsync();
                                            }

                                            if (newContainerType4Container != null)
                                            {
                                                ContainerTable generateContainerId = null;

                                                ContainerTable containerDto = new ContainerTable
                                                {
                                                    Id = 0,
                                                    ContainerId = "",
                                                    ContainerType = newContainerType4Container,
                                                    ContainerTypeId = newContainerType4Container.Id
                                                };

                                                if (newContainerType4Container.ContainerTypes == "G")
                                                {
                                                    generateContainerId = await _containersRepository.AddAsyncGayLord(containerDto);

                                                    if (generateContainerId != null)
                                                    {
                                                        divertLaneExist.ContainerId = generateContainerId.Id;
                                                        await Context.SaveChangesAsync();


                                                    }
                                                }
                                            }
                                        }

                                        if (divertLaneExist.ContainerId != null || divertLaneExist.ContainerId > 0)
                                        {
                                            if(divertLaneExist.DivertLanes >= 1 && divertLaneExist.DivertLanes <= 4)
                                            {
                                                divertLaneExist.ContainerId = null;

                                                Context.DivertLanes.Update(divertLaneExist);
                                            }
                                        }
                                    }
                                    #endregion

                                    divertLaneExist.Status = propertyValue is 0 ? false : true;
                                }
                                else if (propertyName.EndsWith("_full"))
                                {
                                    divertLaneExist.Full = propertyValue is 0 ? false : true;
                                }
                            }
                        }
                    }

                    await Context.SaveChangesAsync();
                }



            }
            catch (Exception ex)
            {
                throw new Exception($"Error: {ex}");
            }
        }

        private async Task deleteMappingSorter(int divertLaneId)
        {
            var entity = await Context.MappingSorters.FirstOrDefaultAsync(x => x.DivertLaneId == divertLaneId);
            Context.MappingSorters.Remove(entity);
            await Context.SaveChangesAsync();
        }
    }
}
