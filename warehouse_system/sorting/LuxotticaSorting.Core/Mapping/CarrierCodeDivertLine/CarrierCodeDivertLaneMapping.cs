using LuxotticaSorting.Core.CarrierCodes;
using LuxotticaSorting.Core.DivertLanes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.Core.Mapping.CarrierCodeDivertLine
{
    public class CarrierCodeDivertLaneMapping
    {
        [Key]
        public int Id {  get; set; }
        public int CarrierCodeId { get; set; }
        public int DivertLaneId { get; set; }
        public bool Status { get; set; }

        public CarrierCode CarrierCode { get; set; }
        public DivertLane DivertLane { get; set; }
    }
}
