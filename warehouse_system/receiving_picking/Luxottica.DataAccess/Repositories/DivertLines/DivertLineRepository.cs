using Luxottica.Core.Entities.DivertLines;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Luxottica.DataAccess.Repositories.DivertLines
{
    public class DivertLineRepository : Repository<int, DivertLine>
    {
        public DivertLineRepository(V10Context context) : base(context)
        {

        }

        public override async Task<DivertLine> UpdateAsync(DivertLine divertLine)
        {
            var existingEntity = await Context.DivertLines.FirstOrDefaultAsync(x => x.DivertLineValue == divertLine.DivertLineValue && x.Id != divertLine.Id);
            if (existingEntity != null)
            {
                throw new Exception("The DivertLineValue already exists.");
            }
            var entity = await Context.DivertLines.FindAsync(divertLine.Id);

            if (entity == null)
            {
                throw new Exception("The DivertLine with the specified Id does not exist.");
            }

            var jackpotEntity = await Context.JackpotLines.FirstOrDefaultAsync(x => x.DivertLineId == divertLine.Id && x.JackpotLineValue == true);
            var pickingEntity = await Context.PickingJackpotLines.FirstOrDefaultAsync(x => x.DivertLineId == divertLine.Id && x.PickingJackpotLineValue == true);
            if (divertLine.StatusLine == false)
            {
                
                if (jackpotEntity != null || pickingEntity != null)
                {
                    var title = jackpotEntity != null ? "Jackpot" : "Picking Jackpot";
                    throw new Exception($"StatusLine cannot be set to false because this DivertLine is assigned to a {title}.");
                }
            }

            entity.DivertLineValue = divertLine.DivertLineValue;
            entity.StatusLine = divertLine.StatusLine;

            await Context.SaveChangesAsync();

            return entity;
        }

        public override async Task<DivertLine> AddAsync(DivertLine divertLine)
        {
            var entity = await Context.DivertLines.FirstOrDefaultAsync(x => x.DivertLineValue == divertLine.DivertLineValue);
            if (entity != null)
            {
                throw new InvalidOperationException("The value already exists in the database.");
            }

            await Context.DivertLines.AddAsync(divertLine);
            await Context.SaveChangesAsync();

            return divertLine;
        }

        public override async Task<DivertLine> DeleteAsync(int id)
        {
            var entity = await Context.DivertLines.FindAsync(id);

            if (entity == null)
            {
                throw new Exception("The DivertLine with the specified Id does not exist.");
            }

            var jackpotEntity = await Context.JackpotLines.FirstOrDefaultAsync(x => x.DivertLineId == id && x.JackpotLineValue == true);
            if (jackpotEntity != null)
            {
                throw new Exception("This DivertLine cannot be deleted because it is assigned to a Jackpot with StatusLine set to true.");
            }

            var pickingEntity = await Context.PickingJackpotLines.FirstOrDefaultAsync(x => x.DivertLineId == id && x.PickingJackpotLineValue == true);
            if (pickingEntity != null)
            {
                throw new Exception("This DivertLine cannot be deleted because it is assigned to a Picking Jackpot with StatusLine set to true.");
            }

            Context.DivertLines.Remove(entity);
            await Context.SaveChangesAsync();

            return entity;
        }
    }
}
