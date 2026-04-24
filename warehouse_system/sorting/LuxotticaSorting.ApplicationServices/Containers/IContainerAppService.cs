using LuxotticaSorting.ApplicationServices.Shared.DTO.Containers;
using LuxotticaSorting.ApplicationServices.Shared.DTO.DivertLanes;
using LuxotticaSorting.DataAccess.Repositories.DivertLanes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Containers
{
    public interface IContainerAppService
    {
        Task<List<ContainerDTO>> GetContainersAsync();
        Task<int> AddContainerAsync(ContainerAddDTO containerAddDTO);
        Task DeleteContainerAsync(int containerId);
        Task<ContainerDTO> GetContainerByIdAsync(int containerId);
        Task EditContainerAsync(int id, ContainerAddDTO containerAddDTO);
        Task<int> AddContainerGayLordAsync(ContainerAddOneStepDTO containerAddOneStepDTO);
        Task<int> AddContainerTruckAsync(ContainerAddOneStepDTO containerAddOneStepDTO);
        Task<List<ContainerToShow>> GetContainersTruckAsync();
        Task<List<ContainerToShow>> GetContainersGaylordAsync();
        Task<bool> AddContainerToPrintAsync(ContainerToPrint containerAddDTO);
    }
}
