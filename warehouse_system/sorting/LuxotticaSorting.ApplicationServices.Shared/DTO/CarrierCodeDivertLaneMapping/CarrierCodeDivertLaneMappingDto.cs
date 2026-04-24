using LuxotticaSorting.Core.CarrierCodes;
using LuxotticaSorting.Core.DivertLanes;
using System.ComponentModel.DataAnnotations;

namespace LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodeDivertLaneMapping
{
    public class CarrierCodeDivertLaneMappingDto
    {
        public int Id { get; set; }
        public int CarrierCodeId { get; set; }
        public int DivertLaneId { get; set; }
        public bool Status { get; set; }

        
    }
}
