using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luxottica.Core.Entities.JackpotLines;
using Microsoft.EntityFrameworkCore;

namespace Luxottica.DataAccess.Repositories.JackpotLines
{
    public class JackpotLineRepository : Repository<int, Core.Entities.JackpotLines.JackpotLine>
    {
        public JackpotLineRepository(V10Context context) : base(context)
        {
        }
        public override async Task<JackpotLine> AddAsync(JackpotLine line)
        {
            var divertLine = await Context.DivertLines.FindAsync(line.DivertLine.Id);
            if (divertLine == null)
            {
                throw new Exception("The DivertLine ID does not exist in the database..");
            }

            line.DivertLine = null;

            await Context.JackpotLines.AddAsync(line);

            divertLine.JackpotLines.Add(line);

            await Context.SaveChangesAsync();

            return line;
        }



        public override async Task<JackpotLine> GetAsync(int id)
        {
            var line = await Context.JackpotLines.Include(x => x.DivertLine).FirstOrDefaultAsync(x => x.Id == id);
            return line;
        }

        public override async Task<JackpotLine> UpdateAsync(JackpotLine line)
        {
            var divertLine = await Context.DivertLines.FindAsync(line.DivertLine.Id);
            if (divertLine == null)
            {
                throw new Exception("The DivertLine ID does not exist in the database..");
            }

            var entity = await Context.JackpotLines.FindAsync(line.Id);
            var state = line.JackpotLineValue;

            if (line.JackpotLineValue)
            {
                var Jackline = from m in Context.JackpotLines select m;
                foreach (var j in Jackline)
                {
                    j.JackpotLineValue = false;
                }
                Context.JackpotLines.UpdateRange(Jackline);
            }

            entity.JackpotLineValue = state;
            entity.DivertLine = divertLine;

            await Context.SaveChangesAsync();

            return entity;
        }

        public override IQueryable<JackpotLine> GetAll()
        {
            var line = from m in Context.JackpotLines.Include(x => x.DivertLine) select m;
            return line;
        }
    }
}
