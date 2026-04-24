using LuxotticaSorting.Core.ScanLogSortings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.ScanlogSortings
{
    public interface IScanlogSortingAppService
    {
        Task<List<ScanLogSorting>> GetScanLogSortingAsync();
    }
}
