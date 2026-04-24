using Luxottica.ApplicationServices.Shared.Dto.Scanlogs;
using Luxottica.ApplicationServices.ToteInformations;
using Luxottica.Core.Entities.Scanlogs;
using Luxottica.Core.Entities.ToteInformations;
using Luxottica.DataAccess.Repositories.Scanlogs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.Scanlogs
{
    public class ScanlogsAppService : IScanlogsAppService
    {
        private readonly ILogger<ToteInformationAppService> _logger;
        private readonly ScanlogsRepository _scanlogsRepository;

        public ScanlogsAppService(ILogger<ToteInformationAppService> logger, ScanlogsRepository scanlogsRepository)
        {
            _logger = logger;
            _scanlogsRepository = scanlogsRepository;
        }

        public async Task AddScanlogAsync(ScanlogsAddDto addScanlog)
        {
            try
            {
                await _scanlogsRepository.AddScanlog(addScanlog);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR INSERT ScanlogsReceivingPickings IN ScanlogsAppService, Message {ex.Message}");
                throw new Exception($"AddScanlog unsuccessful. Error: {ex.Message}");
            }
        }

        public async Task<List<ScanlogsReceivingPicking>?> GetAllScanlogsAsync()
        {
            try
            {
                var result = await _scanlogsRepository.GetAllScanlogs();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT ScanlogsReceivingPickings IN ScanlogsAppService, Message {ex.Message}");
                throw new Exception($"GetAllScanlogsAsync unsuccessful. Error: {ex.Message}");
            }
        }        
    }
}
