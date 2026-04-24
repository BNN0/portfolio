using LuxotticaSorting.ApplicationServices.Shared.DTO.TrafficLights;
using LuxotticaSorting.DataAccess.Repositories.TrafficLights;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.TrafficLights
{
    public class TrafficLightAppService : ITrafficLightAppService
    {
        private readonly TrafficLightRepository _traficService;
        private readonly ILogger<TrafficLightAppService> _logger;
        public TrafficLightAppService(TrafficLightRepository traficService, ILogger<TrafficLightAppService> logger)
        {
            _traficService = traficService;
            _logger = logger;
        }

        public async Task<TrafficLightDataDto> GetStatusLightLine2()
        {
            try
            {
                var dto = await _traficService.StatusTrafficLigthTruckLine2();
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"TrafficLightAppService method GetStatusLightLine2 failed, error: {ex.Message}.");
                throw;
            }
        }

        public async Task<TrafficLightDataDto> GetStatusLightLine4()
        {
            try
            {
                var dto = await _traficService.StatusTrafficLigthTruckLine4();
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"TrafficLightAppService method GetStatusLightLine4 failed, error: {ex.Message}");
                throw;
            }
        }
    }
}
