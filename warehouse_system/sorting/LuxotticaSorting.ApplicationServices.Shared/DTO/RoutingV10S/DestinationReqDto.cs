using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Shared.DTO.RoutingV10S
{
    public class DestinationReqDto
    {
        public string Cam_Id { get; set; }
        public string? BoxId { get; set; }
        public int TrackingId { get; set; }
    }
}
