using Luxottica.ApplicationServices.Shared.Dto.Scanlogs;
using Luxottica.Core.Entities.Scanlogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.Scanlogs
{
    public interface IScanlogsAppService
    {
        Task<List<ScanlogsReceivingPicking>?> GetAllScanlogsAsync();
        Task AddScanlogAsync(ScanlogsAddDto addScanlog);
    }
}
