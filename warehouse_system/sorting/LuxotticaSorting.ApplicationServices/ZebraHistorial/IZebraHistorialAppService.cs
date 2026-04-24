using LuxotticaSorting.ApplicationServices.Shared.DTO.Zebra;
using LuxotticaSorting.ApplicationServices.Shared.DTO.ZebraHistorial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.ZebraHistorial
{
    public interface IZebraHistorialAppService
    {
        Task<List<ZebraHistorialDTO>> GetZebraHistorialsAsync();
        Task<ZebraHistorialDTO> GetZebraHistorialByIdAsync(int zebraHistorial);
        Task<List<ZebraHistorialData>> GetZebraHistorialsToRePrintAsync();
        Task DeleteZebraHistorialsAsync();
        Task<List<ZebraHistorialData>> GetZebraHistorialGaylordAsync();
        Task<List<ZebraHistorialData>> GetZebraHistorialDataCombinatedAsync();
        Task<List<ZebraHistorialData>> GetZebraHistorialTruckAsync();
        Task<List<ZebraHistorialData>> GetZebraHistorialToReprintTruckAsync();
    }
}
