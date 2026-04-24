using LuxotticaSorting.Core.WCSRoutingV10;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Shared.DTO.ConfirmationBoxes
{
    public class BoxesfromRoutingRespDto
    {
        public string ContainerId { get; set; }
        public int DivertLane {  get; set; }
        public int Qty {  get; set; }
        public List<WCSRoutingV10> Boxes { get; set; }
    }
}
