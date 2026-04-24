using LuxotticaSorting.Core.CarrierCodes;
using LuxotticaSorting.Core.DivertLanes;

namespace LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodeDivertLaneMapping
{
    public class CarrierCodeDivertLaneMappingAddDto
    {
        public int CarrierCodeId { get; set; }
        public int DivertLaneId { get; set; }
        public bool Status { get; set; }

    }
}
