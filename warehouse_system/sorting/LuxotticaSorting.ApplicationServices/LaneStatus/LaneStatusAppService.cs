using AutoMapper;
using LuxotticaSorting.ApplicationServices.Shared.DTO.LaneStatus;
using LuxotticaSorting.DataAccess.Repositories.DivertLanes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.LaneStatus
{
    public class LaneStatusAppService : ILaneStatusAppService
    {
        private readonly DivertLanesRepository _divertLanesRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<LaneStatusAppService> _logger;

        public LaneStatusAppService(DivertLanesRepository divertLanesRepository, IMapper mapper, ILogger<LaneStatusAppService> logger)
        {
            _divertLanesRepository = divertLanesRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task UpdateLaneStatus(LaneStatusDto laneStatus)
        {
            try
            {
                await _divertLanesRepository.UpdateLaneStatus(laneStatus);
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"LaneStatusAppService method UpdateLaneStatus failed, error: {ex.Message}");
                throw;
            }
        }
    }
}
