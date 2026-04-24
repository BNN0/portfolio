using LuxotticaSorting.ApplicationServices.Shared.DTO.BoxTypes;
using LuxotticaSorting.ApplicationServices.Shared.DTO.DivertLanes;
using LuxotticaSorting.Core.BoxTypes;
using LuxotticaSorting.DataAccess.Repositories.DivertLanes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.DivertLanes
{
    public interface IDivertLanesAppService
    {
        Task<List<DivertLanesDTO>> GetDivertLanesAsync();
        Task<int> AddDivertLaneAsync(DivertLanesAddDto divertLanesAddDto);
        Task DeleteDivertLaneAsync(int divertLanesId);
        Task<DivertLanesDTO> GetDivertLaneByIdAsync(int divertLanesId);
        Task EditDivertLaneAsync(int id, DivertLanesAddDto divertLanesAddDto);
        Task<List<DivertLanesCreationDTO>> GetDivertLanesCreationAsync();
        Task<DivertLanesCreationDTO> GetDivertLaneCreationByIdAsync(int divertLanesId);
        Task<int> AddDivertLaneCreationAsync(DivertLanesAddCreationDTO divertLanesAddDto);

        Task EditDivertLaneCreationAsync(int id, DivertLanesAddCreationDTO divertLanesAddDto);
        Task<List<DivertLanesDTO>> GetDivertLanesToPrintTruckAsync();
        Task<List<DivertLanesDTO>> GetDivertLanesToPrintGaylordAsync();

    }
}
