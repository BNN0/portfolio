using Luxottica.Core.Entities.CameraAssignments;
using Luxottica.Core.Entities.CameraAssignments.SecondLevel;
using Luxottica.Core.Entities.LimitsSettings;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.Core.Entities.Cameras
{
    public class Camera
    {
        [Key]
        public int Id { get; set; }

        public string CamId { get; set; }

        public ICollection<CameraAssignment> CameraAssignments { get; set; }
        public ICollection<SecondLevelCamera> SecondLevelCameras { get; set;}
        public ICollection<LimitSetting> LimitSettingCameras { get; set; }

    }
}
