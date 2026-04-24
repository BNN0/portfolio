using Luxottica.Core.Entities.Cameras;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.Core.Entities.LimitsSettings
{
    public class LimitSetting
    {
        [Key]
        public int Id {  get; set; }

        [Required]
        public int CameraId { get; set; }

        [Required]
        public int MaximumCapacity {  get; set; }
        public int? CounterTote {  get; set; }

        public Camera Camera { get; set; }
    }
}
