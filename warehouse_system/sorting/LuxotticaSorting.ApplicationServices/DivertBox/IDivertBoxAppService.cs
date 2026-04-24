using LuxotticaSorting.ApplicationServices.Shared.DTO.RoutingV10S;
using LuxotticaSorting.Core.WCSRoutingV10;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.DivertBox
{
    public interface IDivertBoxAppService
    {
        Task<DivertBoxRespDto> DivertBox(string boxId, int trackingId);
        Task<bool> DivertConfirm(string div_timestamp, int trackingId);
        Task RegisterHospital(string boxId,int divert);
        Task RegisterAddScanLog(WCSRoutingV10 result);
    }
}
