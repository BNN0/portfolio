using AutoMapper;
using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.Shared.Dto.LogisticAgents;
using LuxotticaSorting.Core.LogisticAgents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.LogisticAgents
{
    public class LogisticAgentAppService : ILogisticAgentAppService
    {
        private readonly IRepository<int, LogisticAgent> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<LogisticAgentAppService> _logger;
        public LogisticAgentAppService(IRepository<int, LogisticAgent> repository, IMapper mapper, ILogger<LogisticAgentAppService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task AddLogisticAgentAsync(LogisticAgentAddDto logisticAgent)
        {
            try
            {
                var l = _mapper.Map<LogisticAgent>(logisticAgent);
                await _repository.AddAsync(l);
            }
            catch (Exception ex)
            {
                _logger.LogError($"LogisticAgentAppService method AddLogisticAgentAsync failed, error: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteLogisticAgentAsync(int logisticAgentId)
        {
            try
            {
                await _repository.DeleteAsync(logisticAgentId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"LogisticAgentAppService method DeleteDivertLineAsync failed for {logisticAgentId}, error: {ex.Message}");
                throw;
            }
        }

        public async Task EditLogisticAgentAsync(int id, LogisticAgentAddDto logisticAgent)
        {
            try
            {
                var l = _mapper.Map<LogisticAgent>(logisticAgent);
                l.Id = id;
                await _repository.UpdateAsync(l);
            }
            catch (Exception ex)
            {
                _logger.LogError($"LogisticAgentAppService method EditLogisticAgentAsync failed for {id}, error: {ex.Message}");
                throw;
            }
        }

        public async Task<LogisticAgentDto> GetLogisticAgentByIdAsync(int logisticAgentId)
        {
            try
            {
                //replace real entity.
                var l = await _repository.GetAsync(logisticAgentId);
                LogisticAgentDto dto = _mapper.Map<LogisticAgentDto>(l);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"LogisticAgentAppService method GetLogisticAgentByIdAsync failed for {logisticAgentId} error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<LogisticAgentDto>> GetLogisticAgentsAsync()
        {
            try
            {
                var l = await _repository.GetAll().ToListAsync();
                //replace real entity.
                List<LogisticAgentDto> logisticAgents = _mapper.Map<List<LogisticAgentDto>>(l);
                return logisticAgents;
            }
            catch (Exception ex)
            {
                _logger.LogError($"LogisticAgentAppService method GetLogisticAgentsAsync failed, error: {ex.Message}");
                throw;
            }
        }
    }
}
