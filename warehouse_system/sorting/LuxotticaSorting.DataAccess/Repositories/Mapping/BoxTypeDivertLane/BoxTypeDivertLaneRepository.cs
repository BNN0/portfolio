using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.Shared.DTO.BoxTypeDivertLaneMapping;
using LuxotticaSorting.ApplicationServices.Shared.DTO.BoxTypes;
using LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodeLogisticAgent;
using LuxotticaSorting.Core.BoxTypes;
using LuxotticaSorting.Core.CarrierCodes;
using LuxotticaSorting.Core.DivertLanes;
using LuxotticaSorting.Core.Mapping.BoxTypeDivertLane;
using LuxotticaSorting.Core.MappingSorter;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.DataAccess.Repositories.Mapping.BoxTypeDivertLane
{
    public class BoxTypeDivertLaneRepository : Repository<int, BoxTypeDivertLaneMapping>
    {
        public BoxTypeDivertLaneRepository(SortingContext context) : base(context)
        {
        }

        public async override Task<BoxTypeDivertLaneMapping> AddAsync(BoxTypeDivertLaneMapping entity)
        {
            try
            {
                var boxTypeExist = await Context.BoxTypes.FindAsync(entity.BoxTypeId);
                var divertLaneExist = await Context.DivertLanes.FindAsync(entity.DivertLaneId);
                var registerExist = await Context.BoxTypeDivertLaneMappings
                                    .FirstOrDefaultAsync(btdl => btdl.BoxTypeId == entity.BoxTypeId && btdl.DivertLaneId == entity.DivertLaneId);

                if (boxTypeExist == null)
                {
                    throw new Exception($"BoxType does not exist.");
                }

                if (divertLaneExist == null)
                {
                    throw new Exception($"DivertLane does not exist.");
                }

                if (registerExist != null)
                {
                    throw new Exception($"The mapping with Box Type and Divert Lane already exists.");
                }

                var addedEntity = await base.AddAsync(entity);

                return addedEntity;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding BoxTypeDivertLaneMapping.");
            }
        }

        public async Task<List<BoxTypeDivertLaneMenuView>> GetAllView()
        {
            var combinedDataList = await (
                from mapping in Context.BoxTypeDivertLaneMappings
                join boxType in Context.BoxTypes on mapping.BoxTypeId equals boxType.Id
                join divertlane in Context.DivertLanes on mapping.DivertLaneId equals divertlane.Id
                select new BoxTypeDivertLaneMenuView
                {
                    Id = mapping.Id,
                    DivertLaneId = divertlane.Id,
                    DivertLaneValue = divertlane.DivertLanes,
                    BoxTypeId = boxType.Id,
                    BoxTypeValue = boxType.BoxTypes
                }
            ).ToListAsync();
            if (combinedDataList == null)
            {
                return null;
            }
            return combinedDataList;

        }
        public override async Task<bool> DeleteAsync(int id)
        {
            var registerExist = await Context.BoxTypeDivertLaneMappings.FindAsync(id);
            var entity = await Context.MappingSorters.FirstOrDefaultAsync
                (x => x.DivertLaneId == registerExist.DivertLaneId &&
                x.BoxTypeId.Contains(registerExist.BoxTypeId.ToString()));

            if (entity != null)
            {
                var divertLaneData = await Context.DivertLanes.FirstOrDefaultAsync(x => x.Id == registerExist.DivertLaneId);

                if (divertLaneData.Status == true)
                {
                    throw new Exception("The action cannot be performed since the line is on.");
                }
            }

            if (registerExist != null)
            {
                await DeleteBoxType(registerExist.BoxTypeId, registerExist.DivertLaneId);
                await base.DeleteAsync(id);
                return true;
            }
            else
            {
                throw new Exception($"This mapping does not exist.");
            }

        }

        public async override Task<BoxTypeDivertLaneMapping> UpdateAsync(BoxTypeDivertLaneMapping entity)
        {
            var boxTypeExist = await Context.BoxTypes.Where(bt => bt.Id == entity.BoxTypeId).Select(bt => bt).FirstOrDefaultAsync() ?? null;
            var divertLaneExist = await Context.DivertLanes.Where(dl => dl.Id == entity.DivertLaneId).Select(dl => dl).FirstOrDefaultAsync() ?? null;
            var registerExist = await Context.BoxTypeDivertLaneMappings.Where(btdl => btdl.BoxTypeId == entity.BoxTypeId && btdl.DivertLaneId == entity.DivertLaneId).Select(btdl => btdl).FirstOrDefaultAsync() ?? null;
            if (boxTypeExist != null && divertLaneExist != null && registerExist == null)
            {
                if (registerExist.BoxTypeId != entity.BoxTypeId)
                {
                    await updateBoxType(registerExist.BoxTypeId, entity.BoxTypeId, registerExist.DivertLaneId);
                    await DeleteBoxType(registerExist.BoxTypeId, registerExist.DivertLaneId);
                }

                var addedEntity = await base.UpdateAsync(entity);

                return addedEntity;
            }
            return null;
        }

        private async Task updateBoxType(int oldBoxType, int newBoxType, int divertlaneId)
        {
            var entity = await Context.MappingSorters.FirstOrDefaultAsync(x => x.DivertLaneId == divertlaneId);
            if (!entity.BoxTypeId.Contains(newBoxType.ToString()))
            {
                StringBuilder stringBuilder = new StringBuilder();


                stringBuilder.Append(",").Append(entity.BoxTypeId);


                string newBoxTypeString = stringBuilder.ToString();

                entity.BoxTypeId = newBoxTypeString;
                await Context.SaveChangesAsync();
            }
        }

        private async Task DeleteBoxType(int boxTypeId, int divertLaneId)
        {
            var entity = await Context.MappingSorters.FirstOrDefaultAsync(x => x.DivertLaneId == divertLaneId && x.BoxTypeId.Contains(boxTypeId.ToString()));
            if (entity != null)
            {
                var divertLaneData = await Context.DivertLanes.FirstOrDefaultAsync(x => x.Id == divertLaneId);

                if (divertLaneData.Status == true)
                {
                    throw new Exception("The action cannot be performed since the line is on.");
                }
                string numbers = entity.BoxTypeId;

                int numberToDelete = boxTypeId;
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
