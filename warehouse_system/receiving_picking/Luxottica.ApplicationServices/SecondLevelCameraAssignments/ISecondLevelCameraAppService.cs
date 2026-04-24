using Luxottica.ApplicationServices.Shared.Dto.SecondLevelCamera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.SecondLevelCameraAssignments
{
    public interface ISecondLevelCameraAppService
    {
        Task<SecondLevelCameraGetDto> GetSecondLevelCameraAsync();
        Task DeleteSecondLevelCamerasAsync(int id);
        Task<bool> ChangeSecondLevelCamera(int id);

        Task<SecondLevelCameraBundleDto> GetCameraAssignmentInfo();
    }
}
