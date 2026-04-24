using LuxotticaSorting.ApplicationServices.Shared.Dto.LogisticAgents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.LogisticAgents
{
    public interface ILogisticAgentAppService
    {
        Task<List<LogisticAgentDto>> GetLogisticAgentsAsync();
        Task AddLogisticAgentAsync (LogisticAgentAddDto logisticAgent);
        Task DeleteLogisticAgentAsync(int logisticAgentId);
        Task<LogisticAgentDto> GetLogisticAgentByIdAsync (int logisticAgentId);
        Task EditLogisticAgentAsync(int id, LogisticAgentAddDto logisticAgent);
    }
}
