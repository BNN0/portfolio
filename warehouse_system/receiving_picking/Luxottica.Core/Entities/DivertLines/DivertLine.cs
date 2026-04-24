using Luxottica.Core.Entities.CameraAssignments;
using Luxottica.Core.Entities.JackpotLines;
using Luxottica.Core.Entities.PhysicalMaps;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.Core.Entities.DivertLines
{
    public class DivertLine
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DivertLineValue { get; set; }

        [Required]
        public bool StatusLine { get; set; }

        public ICollection<CameraAssignment> CameraAssignments { get; set; }
        public ICollection<MapPhysicalVirtualSAP> MapPhysicalVirtualSAPs { get; set; }
        public ICollection<JackpotLine> JackpotLines { get; set; }
        public ICollection<PickingJackpotLine> PickingJackpotLines { get; set; }
    }
}
