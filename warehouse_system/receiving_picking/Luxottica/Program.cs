using Luxottica;
using Luxottica.ApplicationServices;
using Luxottica.ApplicationServices.CameraAssignments;
using Luxottica.ApplicationServices.DivertLines;
using Luxottica.ApplicationServices.PhysicalMaps;
using Luxottica.ApplicationServices.Users;
using Luxottica.Core.Entities.CameraAssignments;
using Luxottica.Core.Entities.DivertLines;
using Luxottica.Core.Entities.PhysicalMaps;
using Luxottica.DataAccess;
using Luxottica.DataAccess.Repositories;
using Luxottica.DataAccess.Repositories.CameraAssignments;
using Luxottica.DataAccess.Repositories.PhysicalMaps;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Luxottica.ApplicationServices.Shared.Config;
using Luxottica.Auth;
using Luxottica.ApplicationServices.ToteInformations;
using Luxottica.Core.Entities.ToteInformations;
using Luxottica.ApplicationServices.Cameras;
using Luxottica.Core.Entities.Cameras;
using Luxottica.ApplicationServices.JackpotLines;
using Luxottica.Core.Entities.JackpotLines;
using Luxottica.DataAccess.Repositories.JackpotLines;
using Luxottica.DataAccess.Repositories.ToteInformation;
using Luxottica.DataAccess.Repositories.DivertLines;
using Serilog;
using Serilog.Events;
using Luxottica.ApplicationServices.Acknowledgments;
using Luxottica.Core.Entities.Acknowledgments;
using Luxottica.Core.Entities.ToteHdrs;
using Luxottica.ApplicationServices.PickingJackpotLines;
using Luxottica.DataAccess.Repositories.SecondLevelCameraAssignments;
using Luxottica.Core.Entities.CameraAssignments.SecondLevel;
using Luxottica.ApplicationServices.SecondLevelCameraAssignments;
using Luxottica.ApplicationServices.LimitSettings;
using Luxottica.Core.Entities.LimitsSettings;
using Luxottica.DataAccess.Repositories.LimitSettings;
using Luxottica.Core.Entities.Commissioners;
using Luxottica.ApplicationServices.Commissioners;
using Luxottica.DataAccess.Repositories.Commissioners;
using Luxottica.Core.Entities.HighwayPikingLanes;
using AutoMapper;
using Luxottica.ApplicationServices.Shared.Dto.HighwayPickingLanes;
using Luxottica.ApplicationServices.HighwayPickingLanes;
using Luxottica.DataAccess.Repositories.HighwayPickingLanes;
using Luxottica.ApplicationServices.DivertOutboundLines;
using Luxottica.DataAccess.Repositories.DivertOutboundLines;
using Luxottica.Core.Entities.DivertOutboundLines;
using Luxottica.DataAccess.Repositories.Camera;
using Luxottica.ApplicationServices.MapDivert;
using Luxottica.DataAccess.Repositories.MapDivert;
using Luxottica.Controllers.Totes.Luxottica.Controllers.PhysicalMaps;
using Luxottica.DataAccess.Repositories.Acknowledgment;
using Luxottica.ApplicationServices.CommissionerPackingLimits;
using Luxottica.DataAccess.Repositories.CommissionerPackingLimits;
using Luxottica.Core.Entities.EXT;
using Luxottica.Core.Entities.Scanlogs;
using Luxottica.DataAccess.Repositories.Scanlogs;
using Luxottica.ApplicationServices.Scanlogs;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
Console.WriteLine("===============================================");
Console.WriteLine("The program has started");
Console.WriteLine("===============================================");
builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(hostingContext.Configuration)
        .MinimumLevel.ControlledBy(new Serilog.Core.LoggingLevelSwitch(Serilog.Events.LogEventLevel.Error))
        .WriteTo.File(
            path: "logs\\logsDatabase-.txt",
            rollingInterval: RollingInterval.Day,
            rollOnFileSizeLimit: true,
            retainedFileCountLimit: null,
            shared: true,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
        .WriteTo.Console()
        .Filter.ByIncludingOnly(evt => evt.Level >= Serilog.Events.LogEventLevel.Error);
});




builder.Services.AddDbContext<V10Context>(options =>
{
    options.UseSqlServer(configuration.GetConnectionString("v10-db"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 10,
            maxRetryDelay: TimeSpan.FromSeconds(60),
            errorNumbersToAdd: null);
        });
});

builder.Services.AddDbContext<SAPContext>(options =>
{
    options.UseSqlServer(configuration.GetConnectionString("SAP_connection"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 10,
            maxRetryDelay: TimeSpan.FromSeconds(60),
            errorNumbersToAdd: null);
        });
});

builder.Services.AddDbContext<ReceivingWMSContext>(options =>
{
    options.UseSqlServer(configuration.GetConnectionString("Receiving_connection"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 10,
            maxRetryDelay: TimeSpan.FromSeconds(60),
            errorNumbersToAdd: null);
        });
});

builder.Services.AddDbContext<E1ExtContext>(options =>
{
    options.UseSqlServer(configuration.GetConnectionString("E1Ext_connetion"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 10,
            maxRetryDelay: TimeSpan.FromSeconds(60),
            errorNumbersToAdd: null);
        });
});



#region Config Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(
    opts =>
    {
        opts.Password.RequireDigit = true;
        opts.Password.RequireLowercase = true;
        opts.Password.RequireUppercase = true;
        opts.Password.RequireNonAlphanumeric = true;
        opts.Password.RequiredLength = 7;
        opts.Password.RequiredUniqueChars = 4;
    })
    .AddEntityFrameworkStores<V10Context>()
    .AddDefaultTokenProviders();
#endregion


builder.Services.AddAutoMapper(typeof(MapperProfile));

builder.Services.Configure<JwtTokenValidationSettings>(builder.Configuration.GetSection("JwtTokenValidationSettings"));

builder.Services.AddTransient<IJwtIssuerOptions, JwtIssuerFactory>();
builder.Services.AddTransient<IDivertLineService, DivertLineService>();
builder.Services.AddTransient<IMapPhysicalAppService, MapPhysicalAppService>();
builder.Services.AddTransient<ICameraAssignmentService, CameraAssignmentService>();
builder.Services.AddTransient<ICameraAppService, CameraAppService>();
builder.Services.AddTransient<IUserAppService, UserAppService>();
builder.Services.AddTransient<IToteInformationAppService, ToteInformationAppService>();
builder.Services.AddTransient<IJackpotLineAppService, JackpotLineAppService>();
builder.Services.AddTransient<IPickingJackpotLineAppService, PickingJackpotLineAppService>();
builder.Services.AddTransient<IAcknowledgmentAppService, AcknowledgmentAppService>();
builder.Services.AddTransient<ISecondLevelCameraAppService, SecondLevelCameraAppService>();
builder.Services.AddTransient<ILimitSettingsAppService, LimitSettingsAppService>();
builder.Services.AddTransient<ICommissionersAppService, CommissionersAppService>();
builder.Services.AddTransient<IHighWayPickingLanesAppService, HighWayPickingLanesAppService>();
builder.Services.AddTransient<IDivertOutboundLine, DivertOutboundLineAppService>();
builder.Services.AddTransient<IMapDivertAppService, MapDivertAppService>();
builder.Services.AddTransient<ICommissionerPackingLimitAppService, ComissionerPackingLimitAppService>();
builder.Services.AddTransient<IScanlogsAppService, ScanlogsAppService>();


builder.Services.AddTransient<ToteInformationRepository>();
builder.Services.AddTransient<MapPhysicalVirtualSAPRepository>();
builder.Services.AddTransient<CameraAssignmentRepository>();
builder.Services.AddTransient<PickingJackpotLineRepository>();
builder.Services.AddTransient<SecondLevelCameraAsignmentRepository>();
builder.Services.AddTransient<HighwayPickingLaneRepository>();
builder.Services.AddTransient<DivertOutboundLineRepository>();
builder.Services.AddTransient<LimitSettingRepository>();
builder.Services.AddTransient<CameraRepository>();
builder.Services.AddTransient<MapDivertRepository>();
builder.Services.AddTransient<AcknowledgmentRepository>();
builder.Services.AddTransient<CommissionerPackingLimitRepository>();
builder.Services.AddTransient<ScanlogsRepository>();

builder.Services.AddTransient<IRepository<int, MapPhysicalVirtualSAP>, MapDivertRepository>();
builder.Services.AddTransient<IRepository<int, HighwayPikingLane>, HighwayPickingLaneRepository>();
builder.Services.AddTransient<IRepository<int, DivertOutboundLine>, DivertOutboundLineRepository>();
builder.Services.AddTransient<IRepository<int, DivertLine>, DivertLineRepository>();
builder.Services.AddTransient<IRepository<int, MapPhysicalVirtualSAP>, MapPhysicalVirtualSAPRepository>();
builder.Services.AddTransient<IRepository<int, CameraAssignment>, CameraAssignmentRepository>();
builder.Services.AddTransient<IRepository<int, Camera>, CameraRepository>();
builder.Services.AddTransient<IRepository<int, Commissioner>, CommissionerRepository>();
builder.Services.AddTransient<IRepository<int, ToteInformationE>, ToteInformationRepository>();
builder.Services.AddTransient<IRepository<int, JackpotLine>, JackpotLineRepository>();
builder.Services.AddTransient<IRepository<int, PickingJackpotLine>, PickingJackpotLineRepository>();
builder.Services.AddTransient<IRepository<int, Acknowledgment>, AcknowledgmentRepository>();
builder.Services.AddTransient<IRepository<int, LimitSetting>, LimitSettingRepository>();
builder.Services.AddTransient<IRepository<int, SecondLevelCamera>, SecondLevelCameraAsignmentRepository>();
builder.Services.AddTransient<IRepository<int, Commissioner_Packing_Limits>, CommissionerPackingLimitRepository>();
builder.Services.AddTransient<IRepository<int, ScanlogsReceivingPicking>, ScanlogsRepository>();

builder.Services.AddTransient<ToteInformationController>();

builder.Services.AddMemoryCache();



var tokenValidationSettings = builder.Services.BuildServiceProvider().GetService<IOptions<JwtTokenValidationSettings>>().Value;
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = tokenValidationSettings.ValidIssuer,
        ValidAudience = tokenValidationSettings.ValidAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenValidationSettings.SecretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("MyCorsPolicy", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{

}

//Descomentar al iniciar por primera vez el proyecto
//app.InitDb();

app.UseHttpsRedirection();

app.UseCors("MyCorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();