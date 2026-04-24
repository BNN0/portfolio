using AutoMapper;
using Luxottica.ApplicationServices.DivertOutboundLines;
using Luxottica.ApplicationServices.Shared.Dto.HighwayPickingLanes;
using Luxottica.Core.Entities.Commissioners;
using Luxottica.Core.Entities.HighwayPikingLanes;
using Luxottica.DataAccess;
using Luxottica.DataAccess.Repositories;
using Luxottica.DataAccess.Repositories.HighwayPickingLanes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static System.Formats.Asn1.AsnWriter;

namespace Luxottica.ApplicationServices.HighwayPickingLanes
{
    public class HighWayPickingLanesAppService : IHighWayPickingLanesAppService
    {
        private readonly IRepository<int, HighwayPikingLane> _repository;
        private readonly IMapper _mapper;
        private readonly HighwayPickingLaneRepository _HighwayPickingLaneRepository;
        private readonly IRepository<int, Commissioner> _repositoryComm;
        private readonly ILogger<HighWayPickingLanesAppService> _logger;
        public HighWayPickingLanesAppService(IRepository<int, HighwayPikingLane> repository, IMapper mapper,
            HighwayPickingLaneRepository highwayPickingLaneRepository, IRepository<int, Commissioner> repositoryComm, ILogger<HighWayPickingLanesAppService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _HighwayPickingLaneRepository = highwayPickingLaneRepository;
            _repositoryComm = repositoryComm;
            _logger = logger;
        }

        public async Task<List<HighwayPickingLanesDTO>> GetHighwayPickingLaneAsync()
        {
            try
            {
                var t = await _repository.GetAll().ToListAsync();
                List<HighwayPickingLanesDTO> highways = _mapper.Map<List<HighwayPickingLanesDTO>>(t);
                return highways;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT HighwayPickingLanes IN GetHighwayPickingLaneAsync Service {ex.Message}");
                throw new Exception($"{ex.Message}");
            }
        }

        public async Task AddHighwayAsync()
        {
            try
            {

                var com = _repositoryComm.GetAll().FirstOrDefault();
                var newLane = new HighwayPikingLane
                {
                    MultiTotes = 20,
                    MaxTotesLPTUMachine1 = 20,
                    MaxTotesSPTAMachine1 = 20,
                    MaxTotesSPTAMachine2 = 20,
                    CountMultiTotes = 0,
                    CountMaxTotesLPTUMachine1 = 0,
                    CountMaxTotesSPTAMachine1 = 0,
                    CountMaxTotesSPTAMachine2 = 0,
                    CommissionerId = com.Id
                };

                var t = _mapper.Map<HighwayPikingLane>(newLane);
                await _repository.AddAsync(t);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR INSERT HighwayPickingLanes IN AddHighwayAsync Service {ex.Message}");
                throw new Exception($"{ex.Message}");
            }
        }

        public async Task<HighwayPickingRequest> UpdateLimits(HighwayPickingRequest request)
        {
            try
            {
                await _HighwayPickingLaneRepository.UpdateLimits(request);
                return request;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR UPDATE HighwayPickingLanes IN UpdateLimits Service {ex.Message}");
                throw new Exception($"{ex.Message}");
            }
        }
    }
}
