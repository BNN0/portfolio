using Luxottica.ApplicationServices.Shared.Dto.CameraAssignments;
using Luxottica.Core.Entities.CameraAssignments;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.DataAccess.Repositories.CameraAssignments
{
    public class CameraAssignmentRepository : Repository<int, CameraAssignment>
    {
        public CameraAssignmentRepository(V10Context context) : base(context)
        {
        }

        public override async Task<CameraAssignment> AddAsync(CameraAssignment cameraAssignment)
        {
            var divertLine = await Context.DivertLines.FirstOrDefaultAsync(x => x.Id == cameraAssignment.DivertLineId);
            var camera = await Context.Cameras.FirstOrDefaultAsync(x => x.Id == cameraAssignment.CameraId);
            if (divertLine == null || camera == null)
            {
                throw new InvalidOperationException("The entity data already exists in the database.");
            }

            var divertLineSearch = await Context.CameraAssignments
                .FirstOrDefaultAsync(x => x.DivertLineId == cameraAssignment.DivertLineId && x.Id != cameraAssignment.Id);

            var cameraSearch = await Context.CameraAssignments
                .FirstOrDefaultAsync(x => x.CameraId == cameraAssignment.CameraId && x.Id != cameraAssignment.Id);

            if (divertLineSearch != null || cameraSearch != null)
            {
                throw new InvalidOperationException("The entity is already registered.");
            }

            cameraAssignment.DivertLine = null;
            cameraAssignment.Camera = null;

            await Context.CameraAssignments.AddAsync(cameraAssignment);

            divertLine.CameraAssignments.Add(cameraAssignment);

            await Context.SaveChangesAsync();

            return cameraAssignment;
        }

        public override async Task<CameraAssignment> GetAsync(int id)
        {
            var cameraAssignment = await Context.CameraAssignments.Include(x => x.DivertLine).Include(x => x.Camera).FirstOrDefaultAsync(x => x.Id == id);
            return cameraAssignment;
        }

        public override async Task<CameraAssignment> UpdateAsync(CameraAssignment cameraAssignment)
        {
            var existingEntity = await Context.CameraAssignments
                .Include(x => x.DivertLine)
                .Include(x => x.Camera)
                .FirstOrDefaultAsync(x => x.Id == cameraAssignment.Id);

            if (existingEntity == null)
            {
                throw new Exception("The CameraAssignment with that Id does not exist.");
            }

            var divertLine = await Context.DivertLines.FindAsync(cameraAssignment.DivertLine.Id);
            var camera = await Context.Cameras.FindAsync(cameraAssignment.Camera.Id);

            if (divertLine == null || camera == null)
            {
                throw new InvalidOperationException("The entity data does not exist in the database.");
            }

            var divertLineSearch = await Context.CameraAssignments
                .FirstOrDefaultAsync(x => x.DivertLineId == cameraAssignment.DivertLineId && x.Id != cameraAssignment.Id);

            var cameraSearch = await Context.CameraAssignments
                .FirstOrDefaultAsync(x => x.CameraId == cameraAssignment.CameraId && x.Id != cameraAssignment.Id);

            if (divertLineSearch != null || cameraSearch != null)
            {
                throw new InvalidOperationException("The entity is already registered.");
            }

            existingEntity.Camera = camera;
            existingEntity.DivertLine = divertLine;

            await Context.SaveChangesAsync();

            return existingEntity;
        }

        public override IQueryable<CameraAssignment> GetAll()
        {
            var cameraAssignment = from m in Context.CameraAssignments.Include(x => x.DivertLine).Include(x => x.Camera) select m;
            if (cameraAssignment == null)
            {
                throw new InvalidOperationException("error loading information");
            }
            return cameraAssignment;
        }

        public async Task<List<CameraAssignmentGetAllDto>> GetAllExtraAsync()
        {
            var query = from ca in Context.CameraAssignments
                        join dl in Context.DivertLines on ca.DivertLineId equals dl.Id
                        select new CameraAssignmentGetAllDto
                        {
                            Id = ca.Id,
                            CameraId = ca.CameraId,
                            DivertLineId = ca.DivertLineId,
                            DivertLineValue = dl.DivertLineValue
                        };

            var result = await query.ToListAsync();
            if (result.Count <= 0)
            {
                throw new InvalidOperationException("error no cameras in the DB");
            }
            return result;
        }
        
        /*
        public override async Task DeleteAsync(int id)
        {
            var cameraAssignment = await Context.CameraAssignments.FindAsync(id);

            if (cameraAssignment == null)
            {
                throw new Exception("CameraAssignment not found.");
            }

            var isSecondLevel = await Context.SecondLevelCameras.AnyAsync(sl => sl.CameraId == cameraAssignment.CameraId);

            if (isSecondLevel)
            {
                throw new Exception("Cannot delete a CameraAssignment with a second-level CameraId.");
            }

            Context.CameraAssignments.Remove(cameraAssignment);
            await Context.SaveChangesAsync();
        }
        */

    }
}
