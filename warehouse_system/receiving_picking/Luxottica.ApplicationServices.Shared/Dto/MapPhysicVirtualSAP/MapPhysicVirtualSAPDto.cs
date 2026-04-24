using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.Shared.Dto.MapPhysicVirtualSAP
{
    public class MapPhysicVirtualSAPDto
    {
        //[Required]
        public int Id { get; set; }

        //[Required]
        public int DivertLineId { get; set; }

        //[Required]
        public int VirtualSAPZoneId { get; set; }
    }
}
