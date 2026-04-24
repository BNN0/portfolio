using Luxottica.Core.Entities.LimitsSettings;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.DataAccess.Repositories.LimitSettings
{
   
    public class LimitSettingRepository : Repository<int, Core.Entities.LimitsSettings.LimitSetting>
    {
        public LimitSettingRepository(V10Context context) : base(context)
        {
        }
        public override async Task<LimitSetting> AddAsync(LimitSetting entity)
        {
            if (entity.MaximumCapacity <= 0)
            {
                throw new InvalidOperationException($"Maximum Capacity must be greater than 0.");
            }

            if (entity.CounterTote < 0)
            {
                throw new InvalidOperationException($"Counter Tote must be equal or greater than 0.");
            }

            var existCamera = await (from c in Context.Cameras
                                     where c.Id == entity.CameraId
                                     select c).FirstOrDefaultAsync() ?? null;

            var existCamerainLimits = await (from c in Context.LimitSettingCameras
                                     where c.CameraId == entity.CameraId
                                     select c).FirstOrDefaultAsync() ?? null;
            if (existCamerainLimits != null)
            {
                var text = existCamera == null ? "Camera already exist in limits" : "Exist";
                throw new InvalidOperationException($"{text} exist in database.");
            }
            if (existCamera == null)
            {
                var text = existCamera == null ? "Camera does not exist " : "Exist";
                throw new InvalidOperationException($"{text} exist in database.");
            }

            entity.Camera = null;

            await Context.LimitSettingCameras.AddAsync(entity);

            existCamera.LimitSettingCameras.Add(entity);

            await Context.SaveChangesAsync();

            return entity;
        }

        public override async Task<LimitSetting> UpdateAsync(LimitSetting entity)
        {
            var existCamera = await (from c in Context.Cameras
                                     where c.Id == entity.CameraId
                                     select c).FirstOrDefaultAsync() ?? null;

            if (existCamera == null)
            {
                var text = existCamera == null ? "Camera does not exist in the database" : "Camera does not exist in the database";
                throw new InvalidOperationException($"{text}.");
            }
            var objecId = await Context.LimitSettingCameras.FindAsync(entity.Id); 
            if (objecId == null)
            {
                throw new InvalidOperationException($"The limitSetting with id: {entity.Id} does not exist!.");
            }
            if(entity.MaximumCapacity <= 0)
            {
                throw new InvalidOperationException($"Maximum Capacity must be greater than 0.");
            }
            if (entity.CounterTote < 0)
            {
                throw new InvalidOperationException($"Counter Tote must be equal or greater than 0.");
            }
            objecId.Id= entity.Id;
            objecId.CameraId = entity.CameraId;
            objecId.MaximumCapacity = entity.MaximumCapacity;
            objecId.CounterTote = entity.CounterTote;

            await Context.SaveChangesAsync();

            return entity;
        }

        public async Task<LimitSetting> GetAsync()
        {
            var limitSetting = await (from slc in Context.LimitSettingCameras
                                select slc).FirstOrDefaultAsync() ?? null;

            //if (camera == null)
            //{
            //    throw new InvalidOperationException($"Camera Assignment does not exist in the database.");
            //}

            return limitSetting;
        }

        public async Task<LimitSetting> GetAsync(int id)
        {
            var limitSetting = await (from slc in Context.LimitSettingCameras
                                      where slc.Id == id
                                      select slc).FirstOrDefaultAsync();

            return limitSetting;
        }

        public override async Task<LimitSetting> DeleteAsync(int id)
        {
            var entity = await Context.LimitSettingCameras.FindAsync(id);

            if (entity == null)
            {
                throw new Exception("The LimitSettings with the specified Id does not exist.");
            }

            Context.LimitSettingCameras.Remove(entity);
            await Context.SaveChangesAsync();

            return entity;
        }

        public async Task<LimitSetting> GetLimitInCam10()
        {
            var cameras = await Context.Cameras.ToListAsync();
            if (!cameras.Any())
            {
                return null;
            }
            var cam10 = cameras.FirstOrDefault(c => c.CamId == "Cam10");
            if (cam10 == null)
            {
                return null;
            }
            var limitSetting = await Context.LimitSettingCameras.FirstOrDefaultAsync(limit => limit.CameraId == cam10.Id);
            if (limitSetting == null)
            {
                var cam10limit = new LimitSetting
                {
                    CameraId = cam10.Id,
                    MaximumCapacity = 10,
                    CounterTote = 0
                };
                await Context.LimitSettingCameras.AddAsync(cam10limit);
                await Context.SaveChangesAsync();
                var limitSettings = await Context.LimitSettingCameras.ToListAsync();
                 limitSetting = limitSettings.FirstOrDefault(limit => limit.CameraId == cam10.Id);
            }

            return limitSetting;
        }

    }
}

