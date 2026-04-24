using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.Shared.Dto.LimitSettings
{
    public class LimitSettingDTO
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CameraId { get; set; }

        [Required]
        public int MaximumCapacity { get; set; }
        public int? CounterTote { get; set; }
    }
}
