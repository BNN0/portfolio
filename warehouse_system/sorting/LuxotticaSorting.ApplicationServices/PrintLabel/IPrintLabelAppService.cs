using AutoMapper;
using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.Containers;
using LuxotticaSorting.ApplicationServices.Shared.DTO.Containers;
using LuxotticaSorting.ApplicationServices.Shared.DTO.PrintLabel;
using LuxotticaSorting.ApplicationServices.Shared.DTO.Zebra;
using LuxotticaSorting.DataAccess.Repositories.Containers;
using LuxotticaSorting.DataAccess.Repositories.PrintLabels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.PrintLabel
{
    public interface IPrintLabelAppService
    {
        Task<List<ZebraConfigurationDTO>> GetZebraConfigurationsAsync();
        Task<int> AddZebraConfigurationAsync(ZebraConfigurationAddDTO zebraConfigurationAddDTO);
        Task DeleteZebraConfigurationAsync(int zebraConfigurationId);
        Task<ZebraConfigurationDTO> GetZebraConfigurationByIdAsync(int zebraConfigurationId);
        Task EditZebraConfigurationAsync(int id, ZebraConfigurationAddDTO zebraConfigurationAddDTO);
        Task<bool> PrintLabelService(PrintLabelDTO printLabelDTO);
        Task<bool> PrintLabelManualService(PrintManualDTO printLabelDTO);
        Task<bool> PrintLabelReprint(PrintLabelReprint printLabelDTO);
    }
}
