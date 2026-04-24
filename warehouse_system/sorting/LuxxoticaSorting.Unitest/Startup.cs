using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.BoxTypes;
using LuxotticaSorting.ApplicationServices.Containers;
using LuxotticaSorting.ApplicationServices.Mapping.CarrierCodeDivertLaneMapping;
using LuxotticaSorting.Controllers.RoutingV10S;
using LuxotticaSorting.Core.BoxTypes;
using LuxotticaSorting.Core.Container;
using LuxotticaSorting.Core.Mapping.CarrierCodeDivertLine;
using LuxotticaSorting.DataAccess;
using LuxotticaSorting.DataAccess.Repositories.BoxTypes;
using LuxotticaSorting.DataAccess.Repositories.Containers;
using LuxotticaSorting.DataAccess.Repositories.Mapping.CarrierCodeDivertLaneMappings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LuxxoticaSorting.Unitest
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

            services.AddDbContext<SortingContext>(options =>
            options.UseInMemoryDatabase("DataTest"));


            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddTransient<IBoxTypeAppService, BoxTypeAppService>();
            services.AddTransient<IContainerAppService, ContainerAppService>();
            services.AddTransient<ICarrierCodeDivertLaneMappingAppService, CarrierCodeDivertLaneMappingAppService>();

            services.AddTransient<BoxTypeRepository>();
            services.AddTransient<ContainersRepository>();

            services.AddTransient<IRepository<int,CarrierCodeDivertLaneMapping>, CarrierCodeDivertLaneMappingRepository>();
            services.AddTransient<IRepository<int, BoxType>, BoxTypeRepository>();
            services.AddTransient<IRepository<int, ContainerTable>, ContainersRepository>();
            services.AddTransient<RoutingV10SController>();

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var dbContext = serviceProvider.GetRequiredService<SortingContext>();

            }

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
