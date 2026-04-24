using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodeDivertLaneMapping;
using LuxotticaSorting.Core.BoxTypes;
using LuxotticaSorting.Core.CarrierCodes;
using LuxotticaSorting.Core.Mapping.CarrierCodeDivertLine;
using LuxotticaSorting.Core.MappingSorter;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;

namespace LuxotticaSorting.DataAccess.Repositories.Mapping.CarrierCodeDivertLaneMappings
{
    public class CarrierCodeDivertLaneMappingRepository : Repository<int, CarrierCodeDivertLaneMapping>
    {
        public CarrierCodeDivertLaneMappingRepository(SortingContext context) : base(context)
        {
        }
        public override async Task<CarrierCodeDivertLaneMapping> AddAsync(CarrierCodeDivertLaneMapping carrierCodeDivertLaneMapping)
        {
            var existingEntry = await Context.CarrierCodeDivertLaneMappings
                .FirstOrDefaultAsync(x => x.DivertLaneId == carrierCodeDivertLaneMapping.DivertLaneId
                                          && x.CarrierCodeId == carrierCodeDivertLaneMapping.CarrierCodeId);

            if (existingEntry != null)
            {
                throw new InvalidOperationException("The entry already exists.");
            }

            var existingCarrier = await Context.CarrierCodes
                .FirstOrDefaultAsync(x => x.Id == carrierCodeDivertLaneMapping.CarrierCodeId);

            if (existingCarrier == null)
            {
                throw new InvalidDataException("The provided carrier code does not exist.");
            }

            var existingDivert = await Context.DivertLanes
                .FirstOrDefaultAsync(x => x.Id == carrierCodeDivertLaneMapping.DivertLaneId);

            if (existingDivert == null)
            {
                throw new InvalidDataException("The provided divert lane does not exist.");
            }

            await Context.CarrierCodeDivertLaneMappings.AddAsync(carrierCodeDivertLaneMapping);
            await Context.SaveChangesAsync();

            return carrierCodeDivertLaneMapping;
        }

        public override async Task<CarrierCodeDivertLaneMapping> UpdateAsync(CarrierCodeDivertLaneMapping carrierCodeDivertLaneMapping)
        {
            var entity = await Context.CarrierCodeDivertLaneMappings.FirstOrDefaultAsync(x => x.Id == carrierCodeDivertLaneMapping.Id);

            if (entity == null)
            {
                throw new Exception("The mapping of Carrier Code and Divert Lane does not exist.");
            }
            
            var divertLaneData = await Context.DivertLanes.FirstOrDefaultAsync(x => x.Id == entity.DivertLaneId);
            if (divertLaneData.Status == true)
            {
                throw new Exception("The action cannot be performed since the line is on.");
            }

            if (carrierCodeDivertLaneMapping.Status == true)
            {
                // Check if CarrierCode has a mapping in CarrierCodeLogisticAgentMappings
                var logisticAgentMapping = await Context.CarrierCodeLogisticAgentMappings
                    .FirstOrDefaultAsync(x => x.CarrierCodeId == entity.CarrierCodeId);

                if (logisticAgentMapping == null)
                {
                    throw new Exception("Cannot activate the mapping because Carrier Code does not have a Logistic Agent assigned.");
                }

                var mappingsWithSameDivertLane = await Context.CarrierCodeDivertLaneMappings
                    .Where(x => x.DivertLaneId == entity.DivertLaneId && x.Status)
                    .ToListAsync();
                var carrierCodeSorter = await Context.MappingSorters.FirstOrDefaultAsync(x => x.DivertLaneId == entity.DivertLaneId);
                if (mappingsWithSameDivertLane.Count >= 1)
                {
                    // More than one mapping is active, check if LogisticAgent is different in mappingsWithSameDivertLane
                    var logisticAgentIdInSorter = await Context.MappingSorters
                        .Where(x => x.DivertLaneId == entity.DivertLaneId)
                        .Select(x => x.LogisticAgentId)
                        .FirstOrDefaultAsync();

                    if (logisticAgentIdInSorter != logisticAgentMapping.LogisticAgentId)
                    {
                        throw new Exception("Cannot activate the mapping because Logistic Agents are different in active mappings for the same Divert Lane.");
                    }

                    // Additional mapping for the Divert Lane, append CarrierCode to existing record in MappingSorter
                    var carrierCodeIds = carrierCodeSorter.CarrierCodeId?.Split(',');

                    // Check if the CarrierCodeId already exists in the list
                    if (carrierCodeIds != null && !carrierCodeIds.Contains(entity.CarrierCodeId.ToString()))
                    {
                        // Append the CarrierCodeId to the list
                        carrierCodeSorter.CarrierCodeId = string.Join(",", carrierCodeIds.Append(entity.CarrierCodeId.ToString()));
                        await Context.SaveChangesAsync();
                    }
                }
                else
                {
                    // If LogisticAgentId and CarrierCodeId are both null, insert both values
                    carrierCodeSorter.CarrierCodeId = entity.CarrierCodeId.ToString();
                    carrierCodeSorter.LogisticAgentId = logisticAgentMapping.LogisticAgentId;
                    await Context.SaveChangesAsync();
                }
            }
            else // Status is false
            {
                var mappingsWithSameDivertLane = await Context.CarrierCodeDivertLaneMappings
                    .Where(x => x.DivertLaneId == entity.DivertLaneId && x.Status)
                    .ToListAsync();

                if (mappingsWithSameDivertLane.Count > 1)
                {
                    // More than one mapping is active, update CarrierCode in MappingSorter
                    var carrierCodeSorter = await Context.MappingSorters.FirstOrDefaultAsync(x => x.DivertLaneId == entity.DivertLaneId);

                    if (carrierCodeSorter != null)
                    {
                        // Split the concatenated CarrierCodeId
                        var carrierCodeIds = carrierCodeSorter.CarrierCodeId?.Split(',');

                        // Check if the CarrierCodeId to remove exists in the list
                        if (carrierCodeIds != null && carrierCodeIds.Contains(entity.CarrierCodeId.ToString()))
                        {
                            // Remove the CarrierCodeId
                            carrierCodeIds = carrierCodeIds.Except(new[] { entity.CarrierCodeId.ToString() }).ToArray();

                            // Concatenate the remaining CarrierCodeIds
                            carrierCodeSorter.CarrierCodeId = string.Join(",", carrierCodeIds);

                            await Context.SaveChangesAsync();
                        }
                    }
                }
                else
                {
                    // Only one mapping is active, remove CarrierCode and LogisticAgent from MappingSorter
                    var carrierCodeLogisticAgent = await Context.CarrierCodeLogisticAgentMappings
                        .FirstOrDefaultAsync(x => x.CarrierCodeId == entity.CarrierCodeId);

                    var carrierCodeSorter = await Context.MappingSorters.FirstOrDefaultAsync(x =>
                                            x.DivertLaneId == entity.DivertLaneId &&
                                            x.LogisticAgentId == carrierCodeLogisticAgent.LogisticAgentId &&
                                            x.CarrierCodeId == entity.CarrierCodeId.ToString());

                    if (carrierCodeSorter != null)
                    {
                        // Remove the CarrierCode from the list
                        var carrierCodeIds = carrierCodeSorter.CarrierCodeId?.Split(',');

                        // Check if the CarrierCodeId exists in the list
                        if (carrierCodeIds != null && carrierCodeIds.Contains(entity.CarrierCodeId.ToString()))
                        {
                            // Remove the CarrierCodeId from the list
                            carrierCodeSorter.CarrierCodeId = string.Join(",", carrierCodeIds.Except(new[] { entity.CarrierCodeId.ToString() }));
                        }

                        // Remove the LogisticAgentId
                        carrierCodeSorter.LogisticAgentId = null;

                        await Context.SaveChangesAsync();
                    }
                }
            }
            entity.Status = carrierCodeDivertLaneMapping.Status;

            await Context.SaveChangesAsync();

            return entity;
        }


        public override async Task<CarrierCodeDivertLaneMapping> DeleteAsync(int id)
        {
            var entity = await Context.CarrierCodeDivertLaneMappings.FindAsync(id);

            if (entity == null)
            {
                throw new Exception("The mapping between Carrier Code and Divert Lane does not exist.");
            }

            if (entity.Status == true)
            {
                throw new Exception("You cannot delete a mapping between CarrierCode and active DivertLane.");
            }
            var carrierCodeSorter = await Context.MappingSorters.FirstOrDefaultAsync(x => x.DivertLaneId == entity.DivertLaneId);
            if (carrierCodeSorter != null)
            {
                await DeleteCarrierCode(entity.CarrierCodeId, entity.DivertLaneId);
            }

            Context.CarrierCodeDivertLaneMappings.Remove(entity);
            await Context.SaveChangesAsync();

            return entity;
        }
        public async Task<List<CarrierCodeDivertLaneMappingGetAllDto>> GetCombinedDataAsync()
        {
            var combinedDataList = (from mapping in Context.CarrierCodeDivertLaneMappings
                                    join carrierCode in Context.CarrierCodes on mapping.CarrierCodeId equals carrierCode.Id
                                    join divertLane in Context.DivertLanes on mapping.DivertLaneId equals divertLane.Id
                                    select new CarrierCodeDivertLaneMappingGetAllDto
                                    {
                                        Id = mapping.Id,
                                        CarrierCodeId = mapping.CarrierCodeId,
                                        CarrierCodes = carrierCode.CarrierCodes,
                                        DivertLaneId = mapping.DivertLaneId,
                                        DivertLanes = divertLane.DivertLanes,
                                        DivertLaneStatus = divertLane.Status,
                                        MappingStatus = mapping.Status
                                    }).ToList();
            if (combinedDataList == null)
            {
                return null;
            }
            return combinedDataList;
        }

        private async Task updateCarrierCode(int divertLane, int newCarrierCode)
        {
            var entity = await Context.MappingSorters.FirstOrDefaultAsync(x => x.DivertLaneId == divertLane);
            if (!entity.CarrierCodeId.Contains(newCarrierCode.ToString()))
            {
                StringBuilder stringBuilder = new StringBuilder(entity.CarrierCodeId);



                stringBuilder.Append(",").Append(newCarrierCode);


                string newcarrier = stringBuilder.ToString();

                entity.CarrierCodeId = newcarrier;
                await Context.SaveChangesAsync();
            }
        }



        private async Task DeleteCarrierCode(int carrierCode, int divertLane)
        {
            var entity = await Context.MappingSorters.FirstOrDefaultAsync(x => x.DivertLaneId == divertLane && x.CarrierCodeId.Contains(carrierCode.ToString()));
            if (entity != null)
            {
                string numbers = entity.CarrierCodeId;

                int numberToDelete = carrierCode;
                if (numbers != null)
                {
                    List<int> numberList = numbers.Split(',')
                                                   .Where(s => int.TryParse(s, out _))
                                                   .Select(int.Parse)
                                                   .ToList();
                    numberList.RemoveAll(n => n == numberToDelete);

                    string updatedNumbers = string.Join(",", numberList);
                    entity.BoxTypeId = updatedNumbers;
                }

                await Context.SaveChangesAsync();
            }
        }
    }
}
