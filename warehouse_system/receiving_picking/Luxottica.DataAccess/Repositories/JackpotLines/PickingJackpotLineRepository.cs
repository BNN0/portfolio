using Luxottica.ApplicationServices.Shared.Dto.PickingJackpotLines;
using Luxottica.Core.Entities.Cameras;
using Luxottica.Core.Entities.DivertLines;
using Luxottica.Core.Entities.JackpotLines;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.DataAccess.Repositories.JackpotLines
{
    public class PickingJackpotLineRepository : Repository<int, PickingJackpotLine>
    {
        public PickingJackpotLineRepository(V10Context context) : base(context)
        {
        }
        public override async Task<PickingJackpotLine> DeleteAsync(int id)
        {
            var entity = await Context.PickingJackpotLines.FindAsync(id);

            if (entity == null)
            {

                return null;
            }

            Context.PickingJackpotLines.Remove(entity);
            await Context.SaveChangesAsync();
            return entity;
        }
        public override async Task<PickingJackpotLine> AddAsync(PickingJackpotLine entity)
        {
            var divertLine = await (from dl in Context.DivertLines
                                    where dl.Id == entity.DivertLineId
                                    select dl).FirstOrDefaultAsync() ?? null;

            if(divertLine == null)
            {
                throw new Exception("DivertLine ID does not exist in the database");
            }

            entity.DivertLine = null;

            await Context.PickingJackpotLines.AddAsync(entity);

            divertLine.PickingJackpotLines.Add(entity);

            await Context.SaveChangesAsync();

            return entity;
        }

        public override async Task<PickingJackpotLine> UpdateAsync(PickingJackpotLine entity)
        {
            var divertLine = await(from dl in Context.DivertLines
                                   where dl.Id == entity.DivertLineId
                                   select dl).FirstOrDefaultAsync() ?? null;

            if (divertLine == null)
            {
                return null;
            }

            var exist = await (from pj in Context.PickingJackpotLines
                               where pj.DivertLineId == entity.DivertLineId
                               select pj).FirstOrDefaultAsync() ?? null;

            if(exist != null)
            {
                return null;
            }

            Context.PickingJackpotLines.Update(entity);
            await Context.SaveChangesAsync();

            return entity;
        }

        public async Task<bool> ChangeDivertLine(int id)
        {
            var divertlines = await Context.DivertLines.ToListAsync();
            if(divertlines == null)
            {
               return false;
            }

            var diverline = divertlines.FirstOrDefault(c => c.Id == id);
            
            if(diverline == null)
            {
                return false;
            }
            if (diverline.StatusLine == false)
            {
                return false;
            }

            var pickingJackpotLine = await Context.PickingJackpotLines.FirstOrDefaultAsync() ?? null;

            if (pickingJackpotLine != null)
            {
                pickingJackpotLine.DivertLineId = id;
                pickingJackpotLine.PickingJackpotLineValue = true;

                await this.UpdateAsync(pickingJackpotLine);

                return true;
            }
            else
            {
                var newPicking = new PickingJackpotLine();
                newPicking.DivertLineId = id;
                newPicking.PickingJackpotLineValue = true;
                await this.AddAsync(newPicking);

                return true;
            }
        }
    }
}
