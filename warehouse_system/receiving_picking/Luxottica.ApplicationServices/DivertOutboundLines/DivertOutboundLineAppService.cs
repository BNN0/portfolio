using AutoMapper;
using Luxottica.ApplicationServices.Shared.Dto.DivertOutboundLines;
using Luxottica.Core.Entities.Commissioners;
using Luxottica.Core.Entities.DivertOutboundLines;
using Luxottica.DataAccess.Repositories;
using Luxottica.DataAccess.Repositories.DivertOutboundLines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Luxottica.ApplicationServices.DivertOutboundLines
{
    public class DivertOutboundLineAppService : IDivertOutboundLine
    {
        private readonly IRepository<int, DivertOutboundLine> _repository;
        private readonly IMapper _mapper;
        private readonly DivertOutboundLineRepository _DivertOutboundLineRepository;
        private readonly IRepository<int, Commissioner> _repositoryComm;
        private readonly ILogger<DivertOutboundLineAppService> _logger;

        public DivertOutboundLineAppService(IRepository<int, DivertOutboundLine> repository, IMapper mapper,
            DivertOutboundLineRepository divertOutboundLineRepository, IRepository<int, Commissioner> repositoryComm, ILogger<DivertOutboundLineAppService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _DivertOutboundLineRepository = divertOutboundLineRepository;
            _repositoryComm = repositoryComm;
            _logger = logger;
        }
        public async Task AddDivertOutboundLineAsync()
        {
            try
            {
                var com = _repositoryComm.GetAll().FirstOrDefault();

                var newLane = new DivertOutboundLine
                {
                    MultiTotes = 12,
                    MaxTotesLPTUMachine1 = 36,
                    MaxTotesSPTAMachine1 = 36,
                    MaxTotesSPTAMachine2 = 36,
                    CountMultiTotes = 0,
                    CountMaxTotesLPTUMachine1 = 0,
                    CountMaxTotesSPTAMachine1 = 0,
                    CountMaxTotesSPTAMachine2 = 0,
                    CommissionerId = com.Id
                };

                var t = _mapper.Map<DivertOutboundLine>(newLane);
                await _repository.AddAsync(t);

                var newLane2 = new DivertOutboundLine
                {
                    MultiTotes = 10,
                    MaxTotesLPTUMachine1 = 30,
                    MaxTotesSPTAMachine1 = 30,
                    MaxTotesSPTAMachine2 = 30,
                    CountMultiTotes = 0,
                    CountMaxTotesLPTUMachine1 = 0,
                    CountMaxTotesSPTAMachine1 = 0,
                    CountMaxTotesSPTAMachine2 = 0,
                    CommissionerId = com.Id
                };
                var t1 = _mapper.Map<DivertOutboundLine>(newLane2);
                await _repository.AddAsync(t);


            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR INSERT DivertOutboundLine IN AddDivertOutboundLineAsync Service {ex.Message}");
                throw new Exception($"{ex.Message}");

            }
        }

        public async Task<List<DivertOutboundLineDTO>> GetDivertOutboundLineAsync()
        {
            try
            {
                var t = await _repository.GetAll().ToListAsync();
                List<DivertOutboundLineDTO> divertOutboundLines = _mapper.Map<List<DivertOutboundLineDTO>>(t);
                return divertOutboundLines;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT DivertOutboundLines IN GetDivertOutboundLineAsync Service {ex.Message}");
                throw new Exception($"{ex.Message}");
            }
        }

        public async Task<DivertOutboundLineRequestDto> GetDivertOutboundLineLimitsPresentAsync()
        {
            try
            {
                var response = await _DivertOutboundLineRepository.GetCommissionerPresentLimits();
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT DivertOutboundLine Present in GetDivertOutboundLineLimitsPresentAsync Service {ex.Message}");
                throw new Exception($"{ex.Message}");
            }
        }

        public async Task<DivertOutboundLineRequestDto> GetDivertLimitsAsync()
        {
            try
            {
                var response = await _DivertOutboundLineRepository.GetLimits();
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT DivertOutboundLine in GetDivertLimitsAsync Service {ex.Message}");
                throw new Exception($"{ex.Message}");
            }
        }

        public async Task<DivertOutboundLineRequestDto> UpdateLimits(DivertOutboundLineRequestDto request)
        {
            try
            {
                await _DivertOutboundLineRepository.UpdateLimits(request);
                return request;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR UPDATE DivertOutboundLine in UpdateLimits Service {ex.Message}");
                throw new Exception($"{ex.Message}");
            }
        }

        public async Task<DivertOutboundLineRequestDto> UpdateLimitsPresent(DivertOutboundLineRequestDto request)
        {
            try
            {
                await _DivertOutboundLineRepository.UpdateLimitsPresent(request);
                return request;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR UPDATE DivertOutboundLine PRESENT in UpdateLimitsPresent Service {ex.Message}");
                throw new Exception($"{ex.Message}");
            }
        }
    }
}

