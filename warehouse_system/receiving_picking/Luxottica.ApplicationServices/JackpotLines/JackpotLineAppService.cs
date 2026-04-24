using AutoMapper;
using Luxottica.ApplicationServices.HighwayPickingLanes;
using Luxottica.ApplicationServices.Shared.Dto.JackpotLines;
using Luxottica.Core.Entities.DivertLines;
using Luxottica.Core.Entities.JackpotLines;
using Luxottica.Core.Entities.ToteInformations;
using Luxottica.DataAccess.Repositories;
using Luxottica.DataAccess.Repositories.ToteInformation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Luxottica.ApplicationServices.JackpotLines
{
    public class JackpotLineAppService : IJackpotLineAppService
    {
        private readonly IRepository<int, JackpotLine> _repository;
        private readonly IRepository<int, DivertLine> _repositoryDivert;
        private readonly IMapper _mapper;
        private readonly ILogger<JackpotLineAppService> _logger;

        public JackpotLineAppService(IRepository<int, JackpotLine> repository, IMapper mapper, IRepository<int, DivertLine> repositoryDivert, ILogger<JackpotLineAppService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _repositoryDivert = repositoryDivert;
            _logger = logger;
        }

        public async Task UpdateJackpotState(JackpotLineAddDto line)
        {
            try
            {
                var jackpot = await _repository.GetAsync(line.Id);
                jackpot.JackpotLineValue = true;
                await _repository.UpdateAsync(jackpot);

                var divert = await _repositoryDivert.GetAsync(line.DivertLineId);
                divert.StatusLine = false;
                await _repositoryDivert.UpdateAsync(divert);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR UPDATE JackpotLine IN UpdateJackpotState Service {ex.Message}");
                throw new Exception("Failed to update Jackpot State");
            }
        }

        public async Task AddJackpotLinesAsync(JackpotLineAddDto line)
        {
            try
            {
                var m = _mapper.Map<JackpotLine>(line);
                m.JackpotLineValue = false;
                m.DivertLineId = line.DivertLineId;
                await _repository.AddAsync(m);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR Insert JackpotLine IN AddJackpotLinesAsync Service {ex.Message}");
                throw new Exception("Failed to add new Jackpot Line");
            }
        }

        public async Task DeleteJackpotLinesAsync(int lineId)
        {
            try
            {
                await _repository.DeleteAsync(lineId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DELETE JackpotLine where Id = {lineId} IN DeleteJackpotLinesAsync Service {ex.Message}");
                throw new Exception($"Failed to delete Jackpot Line with ID {lineId}");
            }
        }

        public async Task EditJackpotLinesAsync(int id, JackpotLineAddDto line)
        {
            try
            {
                var m = _mapper.Map<JackpotLine>(line);
                m.Id = id;
                await _repository.UpdateAsync(m);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR UPDATE JackpotLine where Id = {id} IN EditJackpotLinesAsync Service {ex.Message}");
                throw new Exception($"Failed to edit Jackpot Line with ID {id}");
            }
        }

        public async Task<List<JackpotLineDto>> GetJackpotLinesAsync()
        {
            try
            {
                var m = await _repository.GetAll().ToListAsync();
                List<JackpotLineDto> jackpots = _mapper.Map<List<JackpotLineDto>>(m);
                return jackpots;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT JackpotLine IN GetJackpotLinesAsync Service {ex.Message}");
                throw new Exception("Failed to retrieve Jackpot Lines");
            }
        }

        public async Task<JackpotLineDto> GetJackpotLineByIdAsync(int lineId)
        {
            try
            {
                var m = await _repository.GetAsync(lineId);
                JackpotLineDto dto = _mapper.Map<JackpotLineDto>(m);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT JackpotLine WHERE Id = {lineId} IN GetJackpotLineByIdAsync Service {ex.Message}");
                throw new Exception($"Failed to retrieve Jackpot Line with ID {lineId}");
            }
        }

        public async Task<bool> ChangeDivertline(int id)
        {
            try
            {
                var diverline = await _repositoryDivert.GetAsync(id);
                if (diverline.StatusLine != false)
                {
                    var jackpot = await _repository.GetAll().FirstOrDefaultAsync();

                    if (jackpot == null)
                    {
                        var newJackpot = new JackpotLine();
                        newJackpot.Id = id;
                        newJackpot.DivertLine = diverline;
                        newJackpot.JackpotLineValue = true;
                        await _repository.AddAsync(newJackpot);
                        return true;
                    }
                    else
                    {
                        jackpot.DivertLineId = id;
                        jackpot.JackpotLineValue = true;
                        await _repository.UpdateAsync(jackpot);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR Update JackpotLine WHERE Id = {id} IN ChangeDivertline Service {ex.Message}");
                throw new Exception($"Failed to change Divertline to Jackpot for ID {id}");
            }
        }
    }
}
