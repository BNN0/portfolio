using Luxottica.Core.Entities.Cameras;
using Luxottica.Core.Entities.DivertLines;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.Core.Entities.CameraAssignments
{
    public class CameraAssignment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CameraId { get; set; }

        [Required]
        public int DivertLineId { get; set; }

        public Camera Camera { get; set; }
        public DivertLine DivertLine { get; set; }
    }
}
