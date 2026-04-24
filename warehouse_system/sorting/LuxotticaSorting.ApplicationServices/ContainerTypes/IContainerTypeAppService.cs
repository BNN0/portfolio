using LuxotticaSorting.ApplicationServices.Shared.Dto.ContainerTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.ContainerTypes
{
    public interface IContainerTypeAppService
    {
        Task<List<ContainerTypeDto>> GetContainerTypesAsync();
        Task AddContainerTypeAsync(ContainerTypeAddDto containerType);
        Task DeleteContainerTypeAsync(int containerTypeId);
        Task<ContainerTypeDto> GetContainerTypeByIdAsync(int containerTypeId);
        Task EditContainerTypeAsync(int id, ContainerTypeAddDto containerType);
    }
}
