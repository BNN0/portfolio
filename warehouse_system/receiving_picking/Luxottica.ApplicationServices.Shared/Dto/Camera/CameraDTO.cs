using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.Shared.Dto.Camera
{
    public class CameraDTO
    {
        [Key]
        public int Id { get; set; }

        public string CamId { get; set; }

    }
}
