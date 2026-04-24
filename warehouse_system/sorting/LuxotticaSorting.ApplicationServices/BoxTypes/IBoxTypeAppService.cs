using LuxotticaSorting.ApplicationServices.Shared.DTO.BoxTypes;
using LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodes;
using LuxotticaSorting.Core.BoxTypes;
using LuxotticaSorting.Core.CarrierCodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.BoxTypes
{
    public interface IBoxTypeAppService
    {
        Task<List<BoxType>> GetBoxTypesAsync();
        Task<int> AddBoxTypeAsync(BoxTypesAddDTO boxTypesAddDTO);
        Task DeleteBoxTypeAsync(int boxTypeId);
        Task<BoxType> GetBoxTypeByIdAsync(int boxTypeId);
        Task EditBoxTypeAsync(int id, BoxTypesAddDTO boxTypesAddDTO);
    }
}
