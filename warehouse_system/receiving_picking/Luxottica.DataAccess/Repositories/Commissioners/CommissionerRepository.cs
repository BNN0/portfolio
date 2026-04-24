using Luxottica.Core.Entities.Commissioners;
using Luxottica.Core.Entities.DivertLines;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.DataAccess.Repositories.Commissioners
{
    public class CommissionerRepository : Repository<int, Commissioner>
    {
        public CommissionerRepository(V10Context context) : base(context)
        {

        }
        public async Task UpdateAsync(Commissioner commissioner)
        {
            try
            {
                Context.Entry(commissioner).State = EntityState.Modified;
                await Context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw new Exception("Error when upgrading to commissioner");
            }
        }
    }
}
