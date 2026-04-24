using Luxottica.Core.Entities.Scanlogs;
using Luxottica.ApplicationServices.Shared.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luxottica.ApplicationServices.Shared.Dto.Scanlogs;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;

namespace Luxottica.DataAccess.Repositories.Scanlogs
{
    public class ScanlogsRepository : Repository<int, ScanlogsReceivingPicking>
    {
        private readonly E1ExtContext _e1EXtContext;
        private readonly IMemoryCache _cache;
        private IConfiguration Configuration { get; }
        public ScanlogsRepository(V10Context context, E1ExtContext e1EXtContext, IConfiguration configuration, IMemoryCache cache) : base(context)
        {
            _e1EXtContext = e1EXtContext;
            Configuration = configuration;
            _cache = cache;
        }

        //GetAll
        public async Task<List<ScanlogsReceivingPicking>?> GetAllScanlogs()
        {
            try
            {
                string cacheKey = "AllScanlogs";
                if (!_cache.TryGetValue(cacheKey, out List<ScanlogsReceivingPicking> scanlogs))
                {
                    scanlogs = await (from s in Context.ScanlogsReceivingPickings
                                      orderby s.Id descending
                                      select new ScanlogsReceivingPicking
                                      {
                                          Id = s.Id,
                                          ToteLPN = s.ToteLPN,
                                          VirtualZone = s.VirtualZone,
                                          VirtualTote = s.VirtualTote,
                                          Wave = s.Wave,
                                          TotesInWave = s.TotesInWave,
                                          TotalQty = s.TotalQty,
                                          DestinationArea = s.DestinationArea,
                                          PutStation = s.PutStation,
                                          Status = s.Status,
                                          Release = s.Release,
                                          Processed = s.Processed,
                                          StatusV10 = s.StatusV10,
                                          LapCount = s.LapCount,
                                          TrackingId = s.TrackingId,
                                          Timestamp = s.Timestamp,
                                          CamId = s.CamId,
                                          DivertCode = s.DivertCode,
                                          Info = s.Info
                                      })
                                      .ToListAsync();

                    _cache.Set(cacheKey, scanlogs, TimeSpan.FromDays(5));
                }
                else
                {
                    var maxId = scanlogs.Max(s => s.Id);

                    var latestScanlogs = await (from s in Context.ScanlogsReceivingPickings
                                                where s.Id > maxId
                                                orderby s.Id descending
                                                select new ScanlogsReceivingPicking
                                                {
                                                    Id = s.Id,
                                                    ToteLPN = s.ToteLPN,
                                                    VirtualZone = s.VirtualZone,
                                                    VirtualTote = s.VirtualTote,
                                                    Wave = s.Wave,
                                                    TotesInWave = s.TotesInWave,
                                                    TotalQty = s.TotalQty,
                                                    DestinationArea = s.DestinationArea,
                                                    PutStation = s.PutStation,
                                                    Status = s.Status,
                                                    Release = s.Release,
                                                    Processed = s.Processed,
                                                    StatusV10 = s.StatusV10,
                                                    LapCount = s.LapCount,
                                                    TrackingId = s.TrackingId,
                                                    Timestamp = s.Timestamp,
                                                    CamId = s.CamId,
                                                    DivertCode = s.DivertCode,
                                                    Info = s.Info
                                                })
                                                .ToListAsync();

                   scanlogs.InsertRange(0, latestScanlogs.OrderByDescending(s => s.Id));

                    _cache.Set(cacheKey, scanlogs, TimeSpan.FromDays(5));
                }

                return scanlogs;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error processing request: {ex.Message}");
            }
        }




        //Add
        public async Task AddScanlog(ScanlogsAddDto addScanlog)
        {
            try
            {
                ScanlogsReceivingPicking scanlog = new ScanlogsReceivingPicking
                {
                    ToteLPN = addScanlog?.ToteLPN,
                    VirtualZone = addScanlog?.VirtualZone,
                    VirtualTote = addScanlog?.VirtualTote,
                    Wave = addScanlog?.Wave,
                    TotesInWave = addScanlog?.TotesInWave,
                    TotalQty = addScanlog?.TotalQty,
                    DestinationArea = addScanlog?.DestinationArea,
                    PutStation = addScanlog?.PutStation,
                    Status = addScanlog?.Status,
                    Release = addScanlog?.Release,
                    Processed = addScanlog?.Processed,
                    StatusV10 = addScanlog?.StatusV10,
                    LapCount = addScanlog?.LapCount,
                    TrackingId = addScanlog?.TrackingId,
                    Timestamp = addScanlog?.Timestamp,
                    CamId = addScanlog?.CamId,
                    DivertCode = addScanlog?.DivertCode,
                    Info = addScanlog?.Info
                };

                //Quitar en caso de ser necesario
                if (!string.IsNullOrWhiteSpace(scanlog.ToteLPN) && scanlog.ToteLPN.StartsWith("T"))
                {
                    //var register = await _e1EXtContext.ToteHdrs.Where(hdr => hdr.Tote_LPN == scanlog.ToteLPN && hdr.Processed == null).OrderBy(hdr => hdr.Timestamp).Select(hdr => hdr).FirstOrDefaultAsync() ?? null;
                    
                    string query = $"SELECT TOP(1) * FROM Tote_Hdr WITH (NOLOCK) WHERE Tote_LPN = '{scanlog.ToteLPN}' AND Wave_Nr = '{scanlog.Wave}' ORDER BY Timestamp ASC;";
                    using (SqlConnection connection = new SqlConnection(Configuration.GetConnectionString("E1Ext_connetion")))
                    {
                        await connection.OpenAsync();
                        SqlCommand command = new SqlCommand(query, connection);
                        var scanlogD = await command.ExecuteReaderAsync();
                        if (scanlogD.Read())
                        {
                            scanlog.Wave = scanlogD["Wave_Nr"] is DBNull ? null : (string)scanlogD["Wave_Nr"];
                            
                            scanlog.TotesInWave = (int)scanlogD["Nr_Of_Totes_In_Wave"];
                            scanlog.TotalQty = (int)scanlogD["Tote_total_Qty"];
                            scanlog.DestinationArea = (string)scanlogD["WCS_Destination_Area"];
                           
                            scanlog.PutStation = scanlogD["Put_Station_Nr"] is DBNull ? null : (int)scanlogD["Put_Station_Nr"];
                            scanlog.Status = scanlogD["Status"] is DBNull ? null : (int)scanlogD["Status"];
                            scanlog.Release = scanlogD["Release"] is DBNull ? null : (int)scanlogD["Release"];
                            scanlog.Processed = scanlogD["Processed"] is DBNull ? null : (bool)scanlogD["Processed"];
                        }
                        await connection.CloseAsync();
                    }
                }
                await Context.ScanlogsReceivingPickings.AddAsync(scanlog);
                await Context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await Context.ScanlogsReceivingPickings.AddAsync(new ScanlogsReceivingPicking
                {
                    Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    Info = "Error processing scanlog. Something wrong with insert information" 
                });
            }
        }
    }
}
