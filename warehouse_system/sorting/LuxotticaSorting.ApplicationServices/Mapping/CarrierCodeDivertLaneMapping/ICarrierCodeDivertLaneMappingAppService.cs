using LuxotticaSorting.ApplicationServices.Shared.Dto.LogisticAgents;
using LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodeDivertLaneMapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Mapping.CarrierCodeDivertLaneMapping
{
    public interface ICarrierCodeDivertLaneMappingAppService
    {
        Task<List<CarrierCodeDivertLaneMappingDto>> GetCarrierCodeDivertLaneMappingAsync();
        Task AddCarrierCodeDivertLaneMappingAsync(CarrierCodeDivertLaneMappingAddDto CarrierCodeDivertLaneMapping);
        Task DeleteCarrierCodeDivertLaneMappingAsync(int CarrierCodeDivertLaneMappingId);
        Task<CarrierCodeDivertLaneMappingDto> GetCarrierCodeDivertLaneMappingByIdAsync(int CarrierCodeDivertLaneMappingId);
        Task EditCarrierCodeDivertLaneMappingAsync(int id, CarrierCodeDivertLaneMappingEditDto CarrierCodeDivertLaneMapping);
        Task<List<CarrierCodeDivertLaneMappingGetAllDto>> GetCombinedDataAsync();
    }
}
