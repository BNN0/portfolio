using Luxottica.ApplicationServices.Shared.Dto.DivertOutboundLines;
using Luxottica.Core.Entities.DivertOutboundLines;

namespace Luxottica.ApplicationServices.DivertOutboundLines
{
    public interface IDivertOutboundLine
    {
        Task<List<DivertOutboundLineDTO>> GetDivertOutboundLineAsync();
        Task<DivertOutboundLineRequestDto> UpdateLimits(DivertOutboundLineRequestDto request);
        Task AddDivertOutboundLineAsync();

        Task<DivertOutboundLineRequestDto> GetDivertOutboundLineLimitsPresentAsync();

        Task<DivertOutboundLineRequestDto> UpdateLimitsPresent(DivertOutboundLineRequestDto request);
    }
}
