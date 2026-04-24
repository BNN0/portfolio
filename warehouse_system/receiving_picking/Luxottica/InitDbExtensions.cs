using Luxottica.Core.Entities.Commissioners;
using Luxottica.Core.Entities.DivertLines;
using Luxottica.Core.Entities.JackpotLines;
using Luxottica.Core.Entities.LimitsSettings;
using Luxottica.DataAccess;
using Luxottica.DataAccess.Repositories.JackpotLines;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Luxottica
{
    public static class InitDbExtensions
    {
        public static IApplicationBuilder InitDb(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var services = scope.ServiceProvider;
                var userManager = services.GetService<UserManager<IdentityUser>>();
                var roleManager = services.GetService<RoleManager<IdentityRole>>();
                var v10Context = services.GetService<V10Context>();
                if (!userManager.Users.Any())
                {
                    Task.Run(() => InitRoles(roleManager)).Wait();
                     Task.Run(() => InitUsers(userManager)).Wait();
                }
                if (!v10Context.Commissioners.Any())
                {
                    Task.Run(() => InitCommissioner(v10Context)).Wait();
                }
                if (!v10Context.DivertLines.Any())
                {
                    Task.Run(() => InitDiverline(v10Context)).Wait();
                }
                if (!v10Context.JackpotLines.Any())
                {
                    Task.Run(() => InitJackpot(v10Context)).Wait();
                }
                if (!v10Context.PickingJackpotLines.Any())
                {
                    Task.Run(() => InitPicking(v10Context)).Wait();
                }
                //if (!v10Context.LimitSettingCameras.Any())
                //{
                //    Task.Run(() => InitLimitCam10(v10Context)).Wait();
                //}
            }
            return app;
        }
        private static async Task InitRoles(RoleManager<IdentityRole> roleManager)
        {
            try
            {
                var admin = new IdentityRole("Admin");
                await roleManager.CreateAsync(admin);
                var superuser = new IdentityRole("SuperUser");
                await roleManager.CreateAsync(superuser);
                var shippingassociate = new IdentityRole("ShippingAssociate");
                await roleManager.CreateAsync(shippingassociate);

            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private static async Task InitUsers(UserManager<IdentityUser> userManager)
        {

            var shippingassociate = new IdentityUser()
            {
                UserName = "shippingassociate@luxottica.com",
                Email = "shippingassociate@luxottica.com",
                PhoneNumber = "1234567891"
            };
            await userManager.CreateAsync(shippingassociate, "3eJ0eMN@*wl9+");
            await userManager.AddToRoleAsync(shippingassociate, "ShippingAssociate");

            var superuser = new IdentityUser()
            {
                UserName = "superuser@luxottica.com",
                Email = "superuser@luxottica.com",
                PhoneNumber = "1234567891"
            };
            await userManager.CreateAsync(superuser, "3eJ0eMN@*wl9+");
            await userManager.AddToRoleAsync(superuser, "SuperUser");

            var admin = new IdentityUser()
            {
                UserName = "admin@luxottica.com",
                Email = "admin@luxottica.com",
                PhoneNumber = "1234567891"
            };
            await userManager.CreateAsync(admin, "3eJ0eMN@*wl9+");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        private static async Task InitCommissioner(V10Context context)
        {
            try
            {
                var status = new Commissioner()
                {
                    Status = true
                };
                await context.Commissioners.AddAsync(status);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private static async Task InitDiverline(V10Context context)
        {
            try
            {
                var dl = new DivertLine()
                {
                    DivertLineValue = 1,
                    StatusLine = true
                };
                await context.DivertLines.AddAsync(dl);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private static async Task InitJackpot(V10Context context)
        {
            try
            {
                var jackpotLine = new JackpotLine()
                {
                    JackpotLineValue = true,
                    DivertLineId = 1
                };
                await context.JackpotLines.AddAsync(jackpotLine);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private static async Task InitPicking(V10Context context)
        {
            try
            {
                var pickingJackpot = new PickingJackpotLine()
                {
                    PickingJackpotLineValue = true,
                    DivertLineId = 1,
                };
                await context.PickingJackpotLines.AddAsync(pickingJackpot);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /*
        private static async Task InitLimitCam10(V10Context context)
        {
            try
            {
                var limitCam10 = new LimitSetting()
                {
                    CameraId = 10,
                    MaximumCapacity = 10,
                    CounterTote = 0
                };
                await context.LimitSettingCameras.AddAsync(limitCam10);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        */

        public static Int64 ToUnixEpochDate(this DateTime dateTime)
        {
            var result = (Int64)Math.Round((dateTime.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
            return result;
        }
    }
}
