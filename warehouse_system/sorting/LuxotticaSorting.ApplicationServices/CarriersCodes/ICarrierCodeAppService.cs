using LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodes;
using LuxotticaSorting.Core.CarrierCodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.CarriersCodes
{
    public interface ICarrierCodeAppService
    {
        Task<List<CarrierCode>> GetCarrierCodesAsync();
        Task <int> AddCarrierCodeAsync(CarrierCodeAddDto carrierCodeAddDto);
        Task DeleteCarrierCodeAsync(int carrierCodeId);
        Task<CarrierCode> GetCarrierCodeByIdAsync(int carrierCodeId);
        Task EditCarrierCodeAsync(int id, CarrierCodeAddDto carrierCodeAddDto);
    }
}
