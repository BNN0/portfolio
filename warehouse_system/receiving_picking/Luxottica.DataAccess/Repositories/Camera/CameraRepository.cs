using Luxottica.ApplicationServices.Shared.Dto.HighwayPickingLanes;
using Luxottica.Core.Entities.CameraAssignments;
using Luxottica.Core.Entities.Cameras;
using Luxottica.Core.Entities.ToteHdrs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.DataAccess.Repositories.Camera
{
    public class CameraRepository : Repository<int, Core.Entities.Cameras.Camera>
    {
        public CameraRepository(V10Context context) : base(context)
        {
        }
        public async Task AddNewCamera()
        {
            var numberOfCameras = await Context.Cameras
                .CountAsync();
            if (numberOfCameras == 0)
            {
                var newCamera1 = new Core.Entities.Cameras.Camera
                {
                    CamId = "Cam01",
                };
                await Context.Cameras.AddAsync(newCamera1);
                await Context.SaveChangesAsync();
                return;
            }
            numberOfCameras += 1;
            string newCamId = "Cam" + numberOfCameras.ToString("00");

            var newCamera = new Core.Entities.Cameras.Camera
            {
                CamId = newCamId,
            };
            await Context.Cameras.AddAsync(newCamera);
            await Context.SaveChangesAsync();
        }

        public override async Task<Core.Entities.Cameras.Camera> AddAsync(Core.Entities.Cameras.Camera camera)
        {
            await Context.Cameras.AddAsync(camera);
            await Context.SaveChangesAsync();
            return camera;
        }


        public async Task DeleteLastRecord()
        {
            var lastCamera = await Context.Cameras
            .OrderByDescending(c => c.Id)
            .FirstOrDefaultAsync();

            var secondLevel = await Context.SecondLevelCameras
            .FirstOrDefaultAsync(c => c.CameraId == lastCamera.Id);

            if(lastCamera.CamId == "Cam01" || lastCamera.CamId == "Cam02" || lastCamera.CamId == "Cam10")
            {
                throw new Exception("Cannot delete Camera with CamId 'Cam01' or 'Cam02' or 'Cam10'.");
            }

            if(secondLevel == null)
            {
                Context.Cameras.Remove(lastCamera);
                await Context.SaveChangesAsync();
            }
            else
            {
                throw new Exception("This camera is a second level camera, it cannot be deleted.");
            }
        }


    }
}

