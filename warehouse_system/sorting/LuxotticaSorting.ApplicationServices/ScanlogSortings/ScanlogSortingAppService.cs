using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.DivertBox;
using LuxotticaSorting.ApplicationServices.Shared.DTO.Zebra;
using LuxotticaSorting.Core.ScanLogSortings;
using LuxotticaSorting.Core.WCSRoutingV10;
using LuxotticaSorting.DataAccess.Repositories.RoutingV10S;
using LuxotticaSorting.DataAccess.Repositories.ScanLogsSortings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.ScanlogSortings
{
    public class ScanlogSortingAppService : IScanlogSortingAppService
    {
        private readonly IRepository<int, ScanLogSorting> _repository;
        private readonly ScanLogSortingRepository _scanlogRepository;
        private readonly ILogger<IDivertBoxAppService> _logger;
        public ScanlogSortingAppService(IRepository<int, ScanLogSorting> repository, ILogger<IDivertBoxAppService> logger, ScanLogSortingRepository scanlogRepository)
        {
            _repository = repository;
            _scanlogRepository = scanlogRepository;
            _logger = logger;
        }

        public async Task<List<ScanLogSorting>> GetScanLogSortingAsync()
        {
            try
            {
                var c = await _repository.GetAll().ToListAsync();   
                return c;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ScanlogSortingAppService method GetScanLogSortingAsync failed, error: {ex.Message}");
                throw;
            }
        }
    }
}
