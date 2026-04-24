using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodeDivertLaneMapping
{
    public class CarrierCodeDivertLaneMappingGetAllDto
    {
        public int Id { get; set; }
        public int CarrierCodeId { get; set; }
        public string CarrierCodes { get; set; }
        public int DivertLaneId { get; set; }
        public int DivertLanes { get; set; }
        public bool DivertLaneStatus { get; set; }
        public bool MappingStatus { get; set; }
    }
}
