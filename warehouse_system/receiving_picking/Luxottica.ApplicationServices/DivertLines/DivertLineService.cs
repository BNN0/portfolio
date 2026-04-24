using AutoMapper;
using Luxottica.ApplicationServices.Commissioners;
using Luxottica.ApplicationServices.Shared.Dto.DivertLines;
using Luxottica.Core.Entities.DivertLines;
using Luxottica.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Luxottica.ApplicationServices.DivertLines
{
    public class DivertLineService : IDivertLineService
    {
        private readonly IRepository<int, DivertLine> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<DivertLineService> _logger;

        public DivertLineService(IRepository<int, DivertLine> repository, IMapper mapper, ILogger<DivertLineService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task AddDivertLineAsync(DivertLineAddDto divertLine)
        {
            try
            {
                var m = _mapper.Map<DivertLine>(divertLine);
                await _repository.AddAsync(m);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR INSERT DIVERTLINE IN AddDivertLineAsync SERVICE {ex.Message}");
                throw new Exception($"{ex.Message}");
            }
        }

        public async Task DeleteDivertLineAsync(int divertLineId)
        {
            try
            {
                await _repository.DeleteAsync(divertLineId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DELETE DIVERTLINE IN DeleteDivertLineAsync SERVICE {ex.Message}");
                throw new Exception($"{ex.Message}");
            }
        }

        public async Task EditDivertLineAsync(int id, DivertLineAddDto divertLine)
        {
            try
            {
                var m = _mapper.Map<DivertLine>(divertLine);
                m.Id = id;
                await _repository.UpdateAsync(m);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR UPDATE DIVERTLINE IN EditDivertLineAsync SERVICE {ex.Message}");
                throw new Exception($"{ex.Message}");
            }
        }

        public async Task<List<DivertLineDto>> GetDivertLineAsync()
        {
            try
            {
                var m = await _repository.GetAll().ToListAsync();
                List<DivertLineDto> divertLine = _mapper.Map<List<DivertLineDto>>(m);
                return divertLine;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT DIVERTLINE IN GetDivertLineAsync SERVICE {ex.Message}");
                throw new Exception($"{ex.Message}");
            }
        }

        public async Task<DivertLineDto> GetDivertLineByIdAsync(int divertLineId)
        {
            try
            {
                var m = await _repository.GetAsync(divertLineId);
                DivertLineDto dto = _mapper.Map<DivertLineDto>(m);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT DIVERTLINE WHERE Id = {divertLineId} IN GetDivertLineByIdAsync SERVICE {ex.Message}");
                throw new Exception($"{ex.Message}");
            }
        }
    }
}
