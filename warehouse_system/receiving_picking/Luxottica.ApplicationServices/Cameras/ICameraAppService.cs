using Luxottica.ApplicationServices.Shared.Dto.Camera;
using Luxottica.Core.Entities.Cameras;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.Cameras
{
    public interface ICameraAppService
    {
        Task<List<CameraDTO>> GetCamerasAsync();

        Task<int> AddCameraAsync(CameraDTO camera);

        Task DeleteCameraAsync(int cameraId);

        Task<CameraDTO> GetCameraByIdAsync(int cameraId);

        Task EditCameraAsync(CameraDTO camera);

        Task DeleteLastRecord();

        Task AddNewCamera();
    }
}
