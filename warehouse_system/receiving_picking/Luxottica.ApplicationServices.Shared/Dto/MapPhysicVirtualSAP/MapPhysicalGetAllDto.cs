using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.Shared.Dto.MapPhysicVirtualSAP
{
    public class MapPhysicalGetAllDto
    {
        public int Id { get; set; }
        public int DivertLineId { get; set; }
        public int DivertLineValue { get; set; }
        public int VirtualSAPZoneId { get; set; }
        
    }
}
