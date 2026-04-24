using Luxottica.ApplicationServices.Shared.Config;
using Luxottica.DataAccess.Repositories;
using LuxotticaSorting;
using LuxotticaSorting.ApplicationServices;
using LuxotticaSorting.ApplicationServices.BoxTypes;
using LuxotticaSorting.ApplicationServices.CarriersCodes;
using LuxotticaSorting.ApplicationServices.ContainerTypes;
using LuxotticaSorting.ApplicationServices.LogisticAgents;
using LuxotticaSorting.ApplicationServices.Mapping.CarrierCodeDivertLaneMapping;
using LuxotticaSorting.Core.BoxTypes;
using LuxotticaSorting.Core.ContainerTypes;
using LuxotticaSorting.Core.LogisticAgents;
using LuxotticaSorting.Core.Mapping.CarrierCodeDivertLine;
using LuxotticaSorting.Core.CarrierCodes;
using LuxotticaSorting.DataAccess;
using LuxotticaSorting.DataAccess.Repositories.BoxTypes;
using LuxotticaSorting.DataAccess.Repositories.CarrierCodes;
using LuxotticaSorting.DataAccess.Repositories.ContainerTypes;
using LuxotticaSorting.DataAccess.Repositories.LogisticAgents;
using LuxotticaSorting.DataAccess.Repositories.Mapping.CarrierCodeDivertLaneMappings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using LuxotticaSorting.DataAccess.Repositories.DivertLanes;
using LuxotticaSorting.Core.DivertLanes;
using LuxotticaSorting.ApplicationServices.DivertLanes;
using LuxotticaSorting.ApplicationServices.Mapping.BoxTypeDivertLane;
using LuxotticaSorting.Core.Mapping.BoxTypeDivertLane;
using LuxotticaSorting.DataAccess.Repositories.Mapping.BoxTypeDivertLane;
using LuxotticaSorting.Core.Mapping.CarrierCodeLogisticAgent;
using LuxotticaSorting.DataAccess.Repositories.Mapping.CarrierCodeLogisticAgent;
using LuxotticaSorting.ApplicationServices.Mapping.CarrierCodeLogisticAgent;
using LuxotticaSorting.DataAccess.Repositories.MappingSorters;
using LuxotticaSorting.Core.MappingSorter;
using LuxotticaSorting.ApplicationServices.MappingSorter;
using LuxotticaSorting.ApplicationServices.Containers;
using LuxotticaSorting.DataAccess.Repositories.Containers;
using LuxotticaSorting.Core.Container;
using LuxotticaSorting.ApplicationServices.RoutingV10;
using LuxotticaSorting.DataAccess.Repositories.RoutingV10S;
using LuxotticaSorting.Core.WCSRoutingV10;
using LuxotticaSorting.ApplicationServices.NewBoxs;
using LuxotticaSorting.ApplicationServices.Shared.DTO.NewBoxs;
using LuxotticaSorting.DataAccess.Repositories.NewBoxs;
using LuxotticaSorting.DataAccess.Repositories.PrintLabels;
using LuxotticaSorting.ApplicationServices.PrintLabel;
using LuxotticaSorting.Core.Zebra;
using LuxotticaSorting.ApplicationServices.DivertBox;
using LuxotticaSorting.ApplicationServices.Mapping.DivertLaneZebraConfigurations;
using LuxotticaSorting.DataAccess.Repositories.DivertLaneZebraConfigurationMapping;
using LuxotticaSorting.Core.Mapping.DivertLaneZebraConfiguration;
using LuxotticaSorting.ApplicationServices.ZebraHistorial;
using LuxotticaSorting.DataAccess.Repositories.ZebraHistorial;
using LuxotticaSorting.Controllers.RoutingV10S;
using LuxotticaSorting.ApplicationServices.LaneStatus;
using LuxotticaSorting.Core.ConfirmationBoxes;
using LuxotticaSorting.DataAccess.Repositories.ConfirmationBoxes;
using Serilog.Events;
using Serilog;
using LuxotticaSorting.ApplicationServices.ScanlogSortings;
using LuxotticaSorting.DataAccess.Repositories.ScanLogsSortings;
using LuxotticaSorting.Core.ScanLogSortings;
using LuxotticaSorting.ApplicationServices.RecirculationLimits;
using LuxotticaSorting.DataAccess.Repositories.RecirculationLimits;
using LuxotticaSorting.Core.RecirculationLimits;
using LuxotticaSorting.ApplicationServices.MultiBoxWaves;
using LuxotticaSorting.DataAccess.Repositories.MultiBoxWaves;
using LuxotticaSorting.Core.MultiBoxWaves;
using LuxotticaSorting.ApplicationServices.TrafficLights;
using LuxotticaSorting.DataAccess.Repositories.TrafficLights;
using System.Net.WebSockets;
using System.Data.SqlClient;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .MinimumLevel.Information()
    .MinimumLevel.Debug()
    .WriteTo.File("Logs/luxotticaSorting.txt",
            rollingInterval: RollingInterval.Day,
            rollOnFileSizeLimit: true,
            retainedFileCountLimit: null,
            shared: true,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.Console()
    .CreateLogger();
try
{
    Log.Information("---------------------------APP INIT---------------------------");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();
    var configuration = builder.Configuration;

    builder.Services.AddDbContext<SortingContext>(options =>
    {
        options.UseSqlServer(configuration.GetConnectionString("v10_connection"));

    });

    builder.Services.AddDbContext<SAPContext>(options =>
    {
        options.UseSqlServer(configuration.GetConnectionString("SAP_connection"));
    });

    builder.Services.AddDbContext<ShippingContext>(options =>
    {
        options.UseSqlServer(configuration.GetConnectionString("Shipping_connection"));
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
        .AddEntityFrameworkStores<SortingContext>()
        .AddDefaultTokenProviders();
    builder.Services.Configure<JwtTokenValidationSettings>(builder.Configuration.GetSection("JwtTokenValidationSettings"));
    builder.Services.AddTransient<IJwtIssuerOptions, JwtIssuerFactory>();
    #endregion

    builder.Services.AddAutoMapper(typeof(MapperProfile));
    builder.Services.AddTransient<ILogisticAgentAppService, LogisticAgentAppService>();
    builder.Services.AddTransient<IContainerTypeAppService, ContainerTypeAppService>();
    builder.Services.AddTransient<IBoxTypeAppService, BoxTypeAppService>();
    builder.Services.AddTransient<ICarrierCodeDivertLaneMappingAppService, CarrierCodeDivertLaneMappingAppService>();
    builder.Services.AddTransient<ICarrierCodeAppService, CarrierCodeAppService>();
    builder.Services.AddTransient<IDivertLanesAppService, DivertLanesAppService>();
    builder.Services.AddTransient<IBoxTypeDivertLaneAppService, BoxTypeDivertLaneAppService>();
    builder.Services.AddTransient<ICarrierCodeLogisticAgentAppService, CarrierCodeLogisticAgentAppService>();
    builder.Services.AddTransient<IMappingSorterAppService, MappingSorterAppService>();
    builder.Services.AddTransient<IRoutingAppService, RoutingAppService>();
    builder.Services.AddTransient<IContainerAppService, ContainerAppService>();
    builder.Services.AddTransient<INewBoxAppService, NewBoxAppService>();
    builder.Services.AddTransient<IPrintLabelAppService, PrintLabelAppService>();
    builder.Services.AddTransient<IDivertBoxAppService, DivertBoxAppService>();
    builder.Services.AddTransient<IDivertLaneZebraConfigurationAppService, DivertLaneZebraConfigurationAppService>();
    builder.Services.AddTransient<IZebraHistorialAppService, ZebraHistorialAppService>();
    builder.Services.AddTransient<ILaneStatusAppService, LaneStatusAppService>();
    builder.Services.AddTransient<IScanlogSortingAppService, ScanlogSortingAppService>();
    builder.Services.AddTransient<IRecirculationLimitAppService, RecirculationLimitAppService>();
    builder.Services.AddTransient<IMultiBoxWaveAppService, MultiBoxWaveAppService>();
    builder.Services.AddTransient<ITrafficLightAppService, TrafficLightAppService>();

    builder.Services.AddTransient<BoxTypeRepository>();
    builder.Services.AddTransient<CarrierCodeRepository>();
    builder.Services.AddTransient<DivertLanesRepository>();
    builder.Services.AddTransient<BoxTypeDivertLaneRepository>();
    builder.Services.AddTransient<CarrierCodeDivertLaneMappingRepository>();
    builder.Services.AddTransient<CarrierCodeLogisticAgentRepository>();
    builder.Services.AddTransient<MappingSorterRepository>();
    builder.Services.AddTransient<RoutingV10Repository>();
    builder.Services.AddTransient<ContainersRepository>();
    builder.Services.AddTransient<PrintLabelRepository>();
    builder.Services.AddTransient<DivertLaneZebraConfigurationMappingRepository>();
    builder.Services.AddTransient<ZebraHistorialRepository>();
    builder.Services.AddTransient<ConfirmationBoxesRepository>();
    builder.Services.AddTransient<ScanLogSortingRepository>();
    builder.Services.AddTransient<RecirculationLimitRepository>();
    builder.Services.AddTransient<MultiBoxWaveRepository>();
    builder.Services.AddTransient<TrafficLightRepository>();

    builder.Services.AddTransient<IRepository<int, DivertLane>, DivertLanesRepository>();
    builder.Services.AddTransient<IRepository<int, CarrierCode>, CarrierCodeRepository>();
    builder.Services.AddTransient<IRepository<int, BoxType>, BoxTypeRepository>();
    builder.Services.AddTransient<IRepository<int, LogisticAgent>, LogisticAgentRepository>();
    builder.Services.AddTransient<IRepository<int, ContainerType>, ContainerTypeRepository>();
    builder.Services.AddTransient<IRepository<int, CarrierCodeDivertLaneMapping>, CarrierCodeDivertLaneMappingRepository>();
    builder.Services.AddTransient<IRepository<int, BoxTypeDivertLaneMapping>, BoxTypeDivertLaneRepository>();
    builder.Services.AddTransient<IRepository<int, CarrierCodeLogisticAgentMapping>, CarrierCodeLogisticAgentRepository>();
    builder.Services.AddTransient<IRepository<int, MappingSorter>, MappingSorterRepository>();
    builder.Services.AddTransient<IRepository<int, WCSRoutingV10>, RoutingV10Repository>();
    builder.Services.AddTransient<IRepository<int, ContainerTable>, ContainersRepository>();
    builder.Services.AddTransient<IRepository<int, NewBoxAddDto>, NewBoxRepository>();
    builder.Services.AddTransient<IRepository<int, ZebraConfiguration>, PrintLabelRepository>();
    builder.Services.AddTransient<IRepository<int, ZebraHistorial>, ZebraHistorialRepository>();
    builder.Services.AddTransient<IRepository<int, DivertLaneZebraConfigurationMapping>, DivertLaneZebraConfigurationMappingRepository>();
    builder.Services.AddTransient<IRepository<int, ConfirmationBox>, ConfirmationBoxesRepository>();
    builder.Services.AddTransient<IRepository<int, ScanLogSorting>, ScanLogSortingRepository>();
    builder.Services.AddTransient<IRepository<int, RecirculationLimit>, RecirculationLimitRepository>();
    builder.Services.AddTransient<IRepository<int, MultiBoxWave>, MultiBoxWaveRepository>();

    builder.Services.AddTransient<RoutingV10SController>();
    builder.Services.AddScoped<WebSocketHandler>();

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
    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();


    var app = builder.Build();


    // #region ejecucion de SP job cada 3 dias
    // DateTime lastExecutionTime = DateTime.MinValue;
    // //para testear cambiar a FromSeconds(1)
    // var timerInterval = TimeSpan.FromDays(3);
    // var timer = new Timer(ExecuteStoredProcedure, null, TimeSpan.Zero, timerInterval);
    // void ExecuteStoredProcedure(object state)
    // {
    //     //para testear tambien deberia de cambiar esta comprobacion
    //     if (DateTime.UtcNow - lastExecutionTime >= TimeSpan.FromDays(3))
    //     {
    //         using (var scope = app.Services.CreateScope())
    //         {
    //             var context = scope.ServiceProvider.GetRequiredService<SortingContext>();

    //             try
    //             {
    //                 context.Database.ExecuteSqlRaw("EXEC [dbo].[ScanlogArchiving]");
    //                 Console.WriteLine("Procedimiento almacenado ejecutado con éxito.");
    //                 lastExecutionTime = DateTime.UtcNow;
    //             }
    //             catch (Exception ex)
    //             {
    //                 Log.Error($"Error to execute store procedure, Error: {ex.Message}");
    //             }
    //         }
    //     }
    // }
    // #endregion

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {

    }

    app.UseWebSockets();
    app.Use(async (context, next) =>
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            using (var scope = context.RequestServices.CreateScope())
            {
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var webSocketHandler = scope.ServiceProvider.GetRequiredService<WebSocketHandler>();
                await webSocketHandler.HandleWebSocketAsync(webSocket);
            }
        }
        else
        {
            await next();
        }
    });

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseCors("MyCorsPolicy");
    app.MapControllers();

    app.Run();
    Log.Logger.Information("______ APP RUN ______");

}
catch (Exception ex)
{
    Log.Error($"Global Error: {ex.Message}");
}