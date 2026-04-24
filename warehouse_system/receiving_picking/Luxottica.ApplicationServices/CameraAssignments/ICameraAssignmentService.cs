using Luxottica.ApplicationServices.Shared.Dto.CameraAssignments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.CameraAssignments
{
    public interface ICameraAssignmentService
    {
        Task<List<CameraAssignmentDto>> GetCameraAssignmentAsync();
        Task<List<CameraAssignmentGetAllDto>> GetAllExtraAsync();

        Task AddCameraAssignmentAsync(CameraAssignmentAddDto cameraAssignment);

        Task DeleteCameraAssignmentAsync(int cameraAssignmentId);

        Task<CameraAssignmentDto> GetCameraAssignmentByIdAsync(int cameraAssignmentId);

        Task EditCameraAssignmentAsync(int id, CameraAssignmentAddDto cameraAssignment);

        Task<bool> CameraAssignmentExistsAsync(int divertLineId);

        Task<bool> CameraAssignmentExistsEditAsync(int divertLineId, int assigmentId);

    }
}
