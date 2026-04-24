using Luxottica.ApplicationServices.Shared.Dto.SecondLevelCamera;
using Luxottica.Core.Entities.CameraAssignments.SecondLevel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Luxottica.DataAccess.Repositories.SecondLevelCameraAssignments
{
    public class SecondLevelCameraAsignmentRepository : Repository<int, SecondLevelCamera>
    {
        public SecondLevelCameraAsignmentRepository(V10Context context) : base(context)
        {
        }

        public override async Task<SecondLevelCamera> AddAsync(SecondLevelCamera entity)
        {
            
            var existCamera = await (from c in Context.Cameras 
                               where c.Id == entity.CameraId
                               select c).FirstOrDefaultAsync() ?? null;

            var existAssignment = await (from slc in Context.SecondLevelCameras
                                         where slc.CameraId == entity.CameraId
                                         select slc).FirstOrDefaultAsync() ?? null;

            if (existCamera == null || existAssignment != null)
            {
                var text = existCamera == null ? "Camera does not " : "Assignment ";
                throw new InvalidOperationException($"{text} exist in database.");
            }

            entity.Camera = null;

            await Context.SecondLevelCameras.AddAsync(entity);

            existCamera.SecondLevelCameras.Add(entity);

            await Context.SaveChangesAsync();

            return entity;
        }

        public override async Task<SecondLevelCamera> UpdateAsync(SecondLevelCamera entity)
        {
            var existCamera = await(from c in Context.Cameras
                                    where c.Id == entity.CameraId
                                    select c).FirstOrDefaultAsync() ?? null;

            var existAssignment = await(from slc in Context.SecondLevelCameras
                                        where slc.CameraId == entity.CameraId
                                        select slc).FirstOrDefaultAsync() ?? null;

            if (existCamera == null || existAssignment != null)
            {
                var text = existCamera == null ? "Camera does not exist in the database" : "Assignment previously made";
                throw new InvalidOperationException($"{text}.");
            }

            Context.SecondLevelCameras.Update(entity);
            await Context.SaveChangesAsync();

            return entity;
        }

        public async Task<SecondLevelCamera> GetAsync()
        {
            var camera = await (from slc in Context.SecondLevelCameras
                                     select slc).FirstOrDefaultAsync() ?? null;

            //if (camera == null)
            //{
            //    throw new InvalidOperationException($"Camera Assignment does not exist in the database.");
            //}

            return camera;
        }

        public async Task<bool> ChangeSecondLevelCameraAssignment(int id)
        {
            var existCamera = await Context.Cameras.FindAsync(id);
            if (existCamera == null)
            {
                return false;
            }

            var cameraAssignment = await Context.SecondLevelCameras.FirstOrDefaultAsync() ?? null;

            if (cameraAssignment != null)
            {
                cameraAssignment.CameraId = id;

                await this.UpdateAsync(cameraAssignment);

                return true;
            }
            else
            {
                var newAssignment = new SecondLevelCamera()
                {
                    CameraId = id,
                };

                await this.AddAsync(newAssignment);
                return true;
            }
        }

        public async Task<SecondLevelCameraBundleDto> GetCameraAssignmentInfo()
        {
            var cameraSecondLevel = await (from slc in Context.SecondLevelCameras
                                           select slc).FirstOrDefaultAsync() ?? null;

            if(cameraSecondLevel != null)
            {
                var cameraInfo = await Context.Cameras.FindAsync(cameraSecondLevel.CameraId);
                if(cameraInfo != null)
                {
                    var bundle = new SecondLevelCameraBundleDto
                    {
                        CameraId = cameraSecondLevel.CameraId,
                        CameraName = cameraInfo.CamId
                    };

                    return bundle;
                }
            }
            return null;
        }
    }
}
