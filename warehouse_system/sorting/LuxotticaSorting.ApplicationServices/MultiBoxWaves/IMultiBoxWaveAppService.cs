using LuxotticaSorting.ApplicationServices.Shared.DTO.MappingSorters;
using LuxotticaSorting.ApplicationServices.Shared.DTO.MultiBoxWaves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.MultiBoxWaves
{
    public interface IMultiBoxWaveAppService
    {
        Task<MultiBoxWavesGetDto> GetMultiBoxWavesAsync(string confirmationNumber);
        Task<(int, bool)> AddMultiBoxWavesAsync(MultiBoxWavesAddDto multiBoxWaves);
        Task<(int, bool)> ConfirmMultiBoxWaveAsync(MultiBoxWavesAddDto multiBoxWaves);
        Task DeleteMultiBoxWavesAsync(int multiBoxWavesId);
        Task<List<MultiBoxWavesGetDto>> GetAllMultiBoxWavesAsync();
        Task<List<MultiBoxWavesGetAllDto>> GetAllBoxMultiBoxWaveAsync();
        Task<bool> ManualConfirmationMultiBoxWavesForTrucks(MultiBoxWaveConfirmationDto multiBoxWaveConfirmation);

        Task<int?> MaxCountQtyConfiguration(int MaxCountQty);
        Task<int?> GetMaxCountQtyConfiguration();

    }
}
