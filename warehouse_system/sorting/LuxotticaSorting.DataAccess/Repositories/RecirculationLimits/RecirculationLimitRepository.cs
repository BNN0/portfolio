using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.Shared.DTO.RecirculationLimits;
using LuxotticaSorting.Core.BoxTypes;
using LuxotticaSorting.Core.RecirculationLimits;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.DataAccess.Repositories.RecirculationLimits
{
    public class RecirculationLimitRepository : Repository<int, RecirculationLimit>
    {
        public RecirculationLimitRepository(SortingContext context) : base(context)
        {
        }

        public async Task<List<RecirculationLimitDto>> GetValues()
        {
            var recirculationLimitValue = await Context.RecirculationLimits.FirstOrDefaultAsync();
            if (recirculationLimitValue == null)
            {
                var recirculationAdd = new RecirculationLimit();
                recirculationAdd.CountLimit = 30;
                await Context.RecirculationLimits.AddAsync(recirculationAdd);
                await Context.SaveChangesAsync();
            }
            var recirculationLimitValueGet = from r in Context.RecirculationLimits
                                          select new RecirculationLimitDto
                                          {
                                              Id = r.Id,
                                              CountLimit = r.CountLimit,
                                          };
            var result = await recirculationLimitValueGet.ToListAsync();
            return result;
        }

        public override async Task<RecirculationLimit> UpdateAsync(RecirculationLimit recirculationLimitAdd)
        {
            var recirculationLimits = await Context.RecirculationLimits.FirstOrDefaultAsync();

            if (recirculationLimitAdd.CountLimit == 0)
            {
                throw new Exception("The value CountLimit must be greater than 0");
            }
            recirculationLimits.CountLimit = recirculationLimitAdd.CountLimit;
            Context.Update(recirculationLimits);
            await Context.SaveChangesAsync();
            return recirculationLimits;
        }
    }
}
