using LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodeDivertLaneMapping;
using LuxotticaSorting.ApplicationServices.Shared.DTO.DivertLaneZebraConfigurationMapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Mapping.DivertLaneZebraConfigurations
{
    public interface IDivertLaneZebraConfigurationAppService
    {
        Task<List<DivertLaneZebraConfigurationMappingDTO>> GetDivertLaneZebraConfigurationMappingAsync();
        Task AddDivertLaneZebraConfigurationMappingAsync(DivertLaneZebraConfigurationMappingAddDTO divertLaneZebraConfigurationMappingAddDTO);
        Task DeleteDivertLaneZebraConfigurationMappingAsync(int divertlaneZebraMappingId);
        Task<DivertLaneZebraConfigurationMappingDTO> GetDivertLaneZebraConfigurationMappingByIdAsync(int divertlaneZebraMappingId);
        Task EditDivertLaneZebraConfigurationMappingAsync(int id, DivertLaneZebraConfigurationMappingAddDTO CarrierCodeDivertLaneMapping);
        Task<List<DivertLanesZebraConfigurationCombinated>> GetDivertLaneZebraConfigurationMappingCombinatedDataAsync();
    }
}
