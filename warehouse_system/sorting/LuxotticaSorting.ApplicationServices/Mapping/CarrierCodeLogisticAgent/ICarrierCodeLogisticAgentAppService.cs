using LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodeDivertLaneMapping;
using LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodeLogisticAgent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Mapping.CarrierCodeLogisticAgent
{
    public interface ICarrierCodeLogisticAgentAppService
    {
        Task<List<CarrierCodeLogisticAgentDto>> GetCarrierCodeLogisticAgentAsync();
        Task AddCarrierCodeLogisticAgentAsync(CarrierCodeLogisticAgentAddDto CarrierCodeLogisticAgent);
        Task DeleteCarrierCodeLogisticAgentAsync(int CarrierCodeLogisticAgentId);
        Task<CarrierCodeLogisticAgentDto> GetCarrierCodeLogisticAgentByIdAsync(int CarrierCodeLogisticAgentId);
        Task EditCarrierCodeLogisticAgentAsync(int id, CarrierCodeLogisticAgentAddDto CarrierCodeLogisticAgent);
        Task<List<CarrierCodeLogisticAgentGetAllDto>> GetCombinedDataAsync();

    }
}
