using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Shared.DTO.RoutingV10S
{
    public class DivertBoxRespDto
    {
        public int TrackingId { get; set; }
        public int? DivertCode {  get; set; }
        public string BoxId { get; set; }//plc lo ocupa?
    }
}
