using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Luxottica.Core.Entities.PhysicalMaps;
using Luxottica.Core.Entities.CameraAssignments;
using Luxottica.Core.Entities.Cameras;
using Luxottica.Core.Entities.DivertLines;
using Luxottica.Core.Entities.JackpotLines;
using Luxottica.Core.Entities.ToteInformations;
using Luxottica.Core.Entities.ToteHdrs;
using Luxottica.Core.Entities.Acknowledgments;
using Luxottica.Core.Entities.CameraAssignments.SecondLevel;
using Luxottica.Core.Entities.LimitsSettings;
using Luxottica.Core.Entities.Commissioners;
using Luxottica.Core.Entities.HighwayPikingLanes;
using Luxottica.Core.Entities.DivertOutboundLines;
using Luxottica.Core.Entities.Scanlogs;

namespace Luxottica.DataAccess
{
    public class V10Context : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public DbSet<Camera> Cameras { get; set; }
        public DbSet<DivertLine> DivertLines { get; set; }
        public DbSet<JackpotLine> JackpotLines { get; set; }
        public DbSet<CameraAssignment> CameraAssignments { get; set; }
        public DbSet<MapPhysicalVirtualSAP> MapPhysicalVirtualSAP { get; set; }
        public DbSet<ToteInformationE> ToteInformations { get; set; }
        public DbSet<Acknowledgment> Acknowledgments { get; set; }
        public DbSet<PickingJackpotLine> PickingJackpotLines { get; set; }
        public DbSet<SecondLevelCamera> SecondLevelCameras { get; set; }
        public DbSet<LimitSetting> LimitSettingCameras { get; set; }
        public DbSet<Commissioner> Commissioners { get; set; }
        public DbSet<HighwayPikingLane> HighwayPikingLanes { get; set; }
        public DbSet<DivertOutboundLine> DivertOutboundLines { get; set; }
        public DbSet<CountWave> CountWaves { get; set; }
        public DbSet<ScanlogsReceivingPicking> ScanlogsReceivingPickings {  get; set; }
        public V10Context(DbContextOptions<V10Context> options) : base ( options )
        {
            
        }
    }
}
