using Luxottica.Core.Entities.DivertLines;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.Core.Entities.PhysicalMaps
{
    public class MapPhysicalVirtualSAP
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DivertLineId { get; set; }

        [Required]
        public int VirtualSAPZoneId { get; set; }

        public DivertLine DivertLine { get; set; }
    }
}
