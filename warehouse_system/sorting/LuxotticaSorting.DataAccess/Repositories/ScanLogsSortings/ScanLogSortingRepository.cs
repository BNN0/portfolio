using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.Core.ScanLogSortings;
using LuxotticaSorting.Core.Zebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.DataAccess.Repositories.ScanLogsSortings
{
    public class ScanLogSortingRepository : Repository<int, ScanLogSorting>
    {
        public ScanLogSortingRepository(SortingContext context) : base(context)
        {

        }

    }
}
