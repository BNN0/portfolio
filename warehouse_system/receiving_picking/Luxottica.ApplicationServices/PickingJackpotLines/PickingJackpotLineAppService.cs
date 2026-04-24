using AutoMapper;
using Luxottica.ApplicationServices.PhysicalMaps;
using Luxottica.ApplicationServices.Shared.Dto.PickingJackpotLines;
using Luxottica.Core.Entities.JackpotLines;
using Luxottica.Core.Entities.ToteInformations;
using Luxottica.DataAccess.Repositories;
using Luxottica.DataAccess.Repositories.JackpotLines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.PickingJackpotLines
{
    public class PickingJackpotLineAppService : IPickingJackpotLineAppService
    {
        private readonly IRepository<int, PickingJackpotLine> _repository;
        private readonly PickingJackpotLineRepository _pickingJackpotLineRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<PickingJackpotLineAppService> _logger;
        public PickingJackpotLineAppService(IRepository<int, PickingJackpotLine> repository, IMapper mapper, PickingJackpotLineRepository pickingJackpotLineRepository, ILogger<PickingJackpotLineAppService> logger)
        {
            _repository = repository;
            _pickingJackpotLineRepository = pickingJackpotLineRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PickingJackpotLine> DeletePickingJackpotLineAsync(int id)
        {
            try
            {
               var entity = await _pickingJackpotLineRepository.DeleteAsync(id);
                if(entity != null)
                {
                    return entity;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete Picking Jackpot Line with ID {id}");
            }
        }

        public async Task<PickingJackpotLineGetDto> GetPickingJackpotLineAsync(int id)
        {
            try
            {
                var pickingJackpot = await _repository.GetAsync(id);
                var picking = _mapper.Map<PickingJackpotLineGetDto>(pickingJackpot);
                return picking;
            }
            catch(Exception ex)
            {
                _logger.LogError($"Select PickingJackpotLine Where Id = {id} in PickingJackpotLineAppService");
                throw new Exception($"Failed to retrieve Picking Jackpot Line with ID {id}");
            }
        }
        
        public async Task<List<PickingJackpotLineGetDto>> GetPickingJackpotLinesAsync()
        {
            try
            {
                var pickingJackpots = await _repository.GetAll().ToListAsync();
                var pickings = _mapper.Map<List<PickingJackpotLineGetDto>>(pickingJackpots);
                return pickings;
            }
            catch(Exception ex)
            {
                _logger.LogError($"Select PickingJackpotLine in PickingJackpotLineAppService");
                throw new Exception("Failed to retrieve Picking Jackpot Lines");
            }
        }

        public async Task<bool> ChangeDivertLine(int id)
        {
            try
            {
                var value = await _pickingJackpotLineRepository.ChangeDivertLine(id);
                return value;
            }
            catch(Exception ex)
            {
                _logger.LogError($"ChangeDivertLine in PickingJackpotLineAppService");
                throw new Exception($"Failed to change Divertline to Jackpot for ID {id}");
            }
        }
    }
}
