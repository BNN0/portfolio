using Luxottica.ApplicationServices.CameraAssignments;
using Luxottica.ApplicationServices.PhysicalMaps;
using Luxottica.Core.Entities.CameraAssignments;
using Luxottica.Core.Entities.DivertLines;
using Luxottica.ApplicationServices.Users;
using Luxottica.Core.Entities.PhysicalMaps;
using Luxottica.DataAccess;
using Luxottica.DataAccess.Repositories;
using Luxottica.DataAccess.Repositories.CameraAssignments;
using Luxottica.DataAccess.Repositories.PhysicalMaps;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Luxottica.ApplicationServices.Cameras;
using Luxottica.Core.Entities.Cameras;
using Luxottica.ApplicationServices.ToteInformations;
using Luxottica.Core.Entities.ToteInformations;
using Luxottica.DataAccess.Repositories.ToteInformation;
using Microsoft.Extensions.Configuration;
using Luxottica.ApplicationServices.Acknowledgments;
using Luxottica.Core.Entities.Acknowledgments;
using Luxottica.DataAccess.Repositories.JackpotLines;
using Luxottica.ApplicationServices.PickingJackpotLines;
using Luxottica.Core.Entities.JackpotLines;
using Luxottica.ApplicationServices.DivertLines;
using Luxottica.DataAccess.Repositories.DivertLines;
using Luxottica.ApplicationServices.SecondLevelCameraAssignments;
using Luxottica.DataAccess.Repositories.SecondLevelCameraAssignments;
using Luxottica.Core.Entities.CameraAssignments.SecondLevel;
using Luxottica.ApplicationServices.LimitSettings;
using Luxottica.DataAccess.Repositories.LimitSettings;
using Luxottica.Core.Entities.LimitsSettings;
using Luxottica.Core.Entities.Commissioners;
using Luxottica.ApplicationServices.Commissioners;
using Luxottica.Core.Entities.HighwayPikingLanes;
using Luxottica.ApplicationServices.HighwayPickingLanes;
using Luxottica.DataAccess.Repositories.HighwayPickingLanes;
using Luxottica.ApplicationServices.DivertOutboundLines;
using Luxottica.DataAccess.Repositories.DivertOutboundLines;
using Luxottica.Core.Entities.DivertOutboundLines;

namespace Luxottica.UnitTest
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {

            services.AddDbContext<V10Context>(options =>
            options.UseInMemoryDatabase("DataTest"));
            services.AddDbContext<ReceivingWMSContext>(options =>
            options.UseInMemoryDatabase("DataTestReciving"));
            services.AddDbContext<SAPContext>(options =>
            options.UseInMemoryDatabase("DataTestSAP"));

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });
            using (var serviceProvider = services.BuildServiceProvider())
            {
                var dbContext = serviceProvider.GetRequiredService<V10Context>();

                if (!dbContext.Commissioners.Any())
                {
                    Commissioner newCommissioner = new Commissioner
                    {
                        Status = true
                    };

                    dbContext.Commissioners.Add(newCommissioner);
                    dbContext.SaveChanges();
                }
            }

            services.AddTransient<IMapPhysicalAppService, MapPhysicalAppService>();
            services.AddTransient<ICameraAssignmentService, CameraAssignmentService>();
            services.AddTransient<IUserAppService, UserAppService>();
            services.AddTransient<ICameraAppService, CameraAppService>();
            services.AddTransient<IToteInformationAppService, ToteInformationAppService>();
            services.AddTransient<IAcknowledgmentAppService, AcknowledgmentAppService>();
            services.AddTransient<IPickingJackpotLineAppService, PickingJackpotLineAppService>();
            services.AddTransient<IDivertLineService, DivertLineService>();
            services.AddTransient<ISecondLevelCameraAppService, SecondLevelCameraAppService>();
            services.AddTransient<ILimitSettingsAppService, LimitSettingsAppService>();
            services.AddTransient<ICommissionersAppService, CommissionersAppService>();
            services.AddTransient<IHighWayPickingLanesAppService, HighWayPickingLanesAppService>();
            services.AddTransient<IDivertOutboundLine, DivertOutboundLineAppService>();

            services.AddTransient<ToteInformationRepository>();
            services.AddTransient<PickingJackpotLineRepository>();
            services.AddTransient<SecondLevelCameraAsignmentRepository>();
            services.AddTransient<LimitSettingRepository>();
            services.AddTransient<HighwayPickingLaneRepository>();
            services.AddTransient<DivertOutboundLineRepository>();

            services.AddTransient<IRepository<int, HighwayPikingLane>, HighwayPickingLaneRepository>();
            services.AddTransient<IRepository<int, DivertOutboundLine>, DivertOutboundLineRepository>();
            services.AddTransient<IRepository<int, MapPhysicalVirtualSAP>, MapPhysicalVirtualSAPRepository>();
            services.AddTransient<IRepository<int, LimitSetting>, LimitSettingRepository>();
            services.AddTransient<IRepository<int, CameraAssignment>, CameraAssignmentRepository>();
            services.AddTransient<IRepository<int, DivertLine>, Repository<int, DivertLine>>();
            services.AddTransient<IRepository<int, Camera>, Repository<int, Camera>>();
            services.AddTransient<IRepository<int, Acknowledgment>, Repository<int, Acknowledgment>>();
            services.AddTransient<IRepository<int, ToteInformationE>, ToteInformationRepository>();
            services.AddTransient<IRepository<int, Commissioner>, Repository<int, Commissioner>>();
            services.AddTransient<IRepository<int, Core.Entities.JackpotLines.PickingJackpotLine>, PickingJackpotLineRepository>();
            services.AddTransient<IRepository<int, DivertLine>, DivertLineRepository>();
            services.AddTransient<IRepository<int, SecondLevelCamera>, SecondLevelCameraAsignmentRepository>();

            services.AddAutoMapper(typeof(ApplicationServices.MapperProfile));

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
