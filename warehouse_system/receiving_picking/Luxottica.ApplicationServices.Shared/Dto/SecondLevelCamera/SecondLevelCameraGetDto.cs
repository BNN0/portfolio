using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.Shared.Dto.SecondLevelCamera
{
    public class SecondLevelCameraGetDto
    {
        public int Id { get; set; }
        public int CameraId { get; set; }
    }
}
