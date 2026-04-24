using LuxotticaSorting.ApplicationServices.Shared.Dto.MappingSorters;
using LuxotticaSorting.ApplicationServices.Shared.DTO.MappingSorters;

namespace LuxotticaSorting.ApplicationServices.MappingSorter
{
    public interface IMappingSorterAppService
    {
        Task<List<MappingSorterDto>> GetMappingSorterAsync();
        Task AddMappingSorterAsync();
        /*Task DeleteMappingSorterAsync(int MappingSorterId);*/
        Task<MappingSorterDto> GetMappingSorterByIdAsync(int MappingSorterId);
        Task EditMappingSorterAsync(int id, MappingSorterAddDto MappingSorter);
        Task<List<MappingSorterGetAllDto>> GetCombinedDataAsync();

        Task AddMappingDivertLaneAndContainerTypeTruck(MappingSorterAddDivertContainerTyDto mappingDivertLaneContainerTypeTruck);
        Task AddMappingDivertLaneAndContainerTypeGaylord(MappingSorterAddDivertContainerTyDto mappingDivertLaneContainerTypeGaylord);
        Task DeleteDataContainerTypeInMappingSorter(int mapId);
        Task<List<MappingSorterFrontendViewDto>> GetOnlyDivertLaneAndConainerType();
    }
}
