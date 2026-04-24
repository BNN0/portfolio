using Luxottica.Core.Entities.DivertLines;
using Luxottica.Core.Entities.ToteHdrs;
using Luxottica.Core.Entities.ToteInformations;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel;
using System;
using System.Reflection;
using System.Security.Cryptography;

using System.Net.NetworkInformation;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Luxottica.Core.Entities.EXT;
using Luxottica.DataAccess.Repositories.Scanlogs;
using Luxottica.ApplicationServices.Shared.Dto.Scanlogs;

namespace Luxottica.DataAccess.Repositories.ToteInformation
{
    public class ToteInformationRepository : Repository<int, ToteInformationE>
    {
        private readonly ReceivingWMSContext _receivingWMSContext;
        private readonly SAPContext _sapContext;
        private readonly E1ExtContext _e1EXtContext;
        private readonly ScanlogsRepository _scanlogsRepository;
        private IConfiguration Configuration { get; }
        public ToteInformationRepository(V10Context context, ReceivingWMSContext receivingWMSContext, SAPContext sapContext, E1ExtContext e1EXtContext, IConfiguration configuration, ScanlogsRepository scanlogsRepository) : base(context)
        {
            _receivingWMSContext = receivingWMSContext;
            _sapContext = sapContext;
            _e1EXtContext = e1EXtContext;
            Configuration = configuration;
            _scanlogsRepository = scanlogsRepository;
        }

        public async Task<bool> IsJackpotLine(string camId)
        {
            try
            {
                var camInfo = await Context.Cameras.Where(c => c.CamId == camId).FirstOrDefaultAsync();
                var camDivLineId = await Context.CameraAssignments.Where(ca => ca.CameraId == camInfo.Id).Select(ca => ca.DivertLineId).FirstOrDefaultAsync();
                var jackpotDivId = await Context.JackpotLines.Select(jl => jl.DivertLineId).FirstOrDefaultAsync();

                if (camDivLineId == jackpotDivId)
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error processing request: {ex.Message}");
            }
        }


        public void UpdateRange(List<ToteInformationE> totes)
        {
            try
            {
                Context.ToteInformations.UpdateRange(totes);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error processing request: {ex.Message}");
            }
        }

        public async Task<int> DivertToteSingle(string Lpm, string Cam_Id, int trackingId)
        {
            try
            {

                string codeSingleSM = "SMC3";
                string codeSingleSP = "SPAC";
                #region Querys
                var idCam = await (from c in Context.Cameras
                                   where c.CamId == Cam_Id
                                   select c.Id)
                   .FirstOrDefaultAsync();

                var existingEntity2 = await _e1EXtContext.ToteHdrs.Where(hdr => hdr.Tote_LPN == Lpm && hdr.Processed == null)
                    .OrderBy(hdr => hdr.Timestamp)
                    .Select(hdr => hdr)
                    .FirstOrDefaultAsync();

                var result = await (from ti in Context.ToteInformations
                                    where ti.ToteLPN == Lpm && ti.DivertStatus == null
                                    select ti)
                                    .FirstOrDefaultAsync() ?? null;

                #endregion


                if (existingEntity2 == null)
                {
                    #region Scanlog
                    await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                    {
                        ToteLPN = Lpm,
                        VirtualZone = result?.ZoneDivertId,
                        StatusV10 = result?.DivertStatus,
                        LapCount = result?.LineCount,
                        TrackingId = trackingId,
                        Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                        CamId = Cam_Id,
                        DivertCode = 99,
                        Info = "Tote_Hdr registration not found"
                    });
                    #endregion
                    return 99;
                }
                if (!(existingEntity2.Nr_Of_Totes_In_Wave == 1))
                {
                    #region Scanlog
                    await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                    {
                        ToteLPN = Lpm,
                        VirtualZone = result?.ZoneDivertId,
                        VirtualTote = result?.VirtualTote,
                        Wave = existingEntity2.Wave_Nr,
                        TotesInWave = existingEntity2.Nr_Of_Totes_In_Wave,
                        TotalQty = existingEntity2.Tote_Total_Qty,
                        DestinationArea = existingEntity2.WCS_Destination_Area,
                        PutStation = existingEntity2.Put_Station_Nr,
                        Status = existingEntity2.Status,
                        Release = existingEntity2.Release,
                        Processed = existingEntity2.Processed,
                        StatusV10 = result?.DivertStatus,
                        LapCount = result?.LineCount,
                        TrackingId = trackingId,
                        Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                        CamId = Cam_Id,
                        DivertCode = 99,
                        Info = "Tote not part of Singles"
                    });
                    #endregion
                    return 99;
                }

                if (result == null)
                {
                    #region Scanlog
                    await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                    {
                        ToteLPN = Lpm,
                        VirtualZone = result?.ZoneDivertId,
                        VirtualTote = result?.VirtualTote,
                        Wave = existingEntity2.Wave_Nr,
                        TotesInWave = existingEntity2.Nr_Of_Totes_In_Wave,
                        TotalQty = existingEntity2.Tote_Total_Qty,
                        DestinationArea = existingEntity2.WCS_Destination_Area,
                        PutStation = existingEntity2.Put_Station_Nr,
                        Status = existingEntity2.Status,
                        Release = existingEntity2.Release,
                        Processed = existingEntity2.Processed,
                        StatusV10 = result?.DivertStatus,
                        LapCount = result?.LineCount,
                        TrackingId = trackingId,
                        Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                        CamId = Cam_Id,
                        DivertCode = 99,
                        Info = "Tote not registered in V10 system"
                    });
                    #endregion
                    return 99;
                }
                
                if (existingEntity2 != null && existingEntity2.WCS_Destination_Area == codeSingleSM && result != null)
                {
                    var timestampPLC = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    result.DivTimestamp = timestampPLC;
                    result.TrackingId = trackingId;
                    result.LastCam = Cam_Id;
                    Context.Update(result);
                    await Context.SaveChangesAsync();

                    if (Lpm.StartsWith("T"))
                    {
                        #region Scanlog
                        await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                        {
                            ToteLPN = Lpm,
                            VirtualZone = result?.ZoneDivertId,
                            VirtualTote = result?.VirtualTote,
                            Wave = existingEntity2.Wave_Nr,
                            TotesInWave = existingEntity2.Nr_Of_Totes_In_Wave,
                            TotalQty = existingEntity2.Tote_Total_Qty,
                            DestinationArea = existingEntity2.WCS_Destination_Area,
                            PutStation = existingEntity2.Put_Station_Nr,
                            Status = existingEntity2.Status,
                            Release = existingEntity2.Release,
                            Processed = existingEntity2.Processed,
                            StatusV10 = result?.DivertStatus,
                            LapCount = result?.LineCount,
                            TrackingId = trackingId,
                            Timestamp = timestampPLC,
                            CamId = Cam_Id,
                            DivertCode = 2,
                            Info = "Tote Divert Successful"
                        });
                        #endregion
                        return 2;
                    }
                    else
                    {
                        #region Scanlog
                        await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                        {
                            ToteLPN = Lpm,
                            VirtualZone = result?.ZoneDivertId,
                            VirtualTote = result?.VirtualTote,
                            Wave = existingEntity2.Wave_Nr,
                            TotesInWave = existingEntity2.Nr_Of_Totes_In_Wave,
                            TotalQty = existingEntity2.Tote_Total_Qty,
                            DestinationArea = existingEntity2.WCS_Destination_Area,
                            PutStation = existingEntity2.Put_Station_Nr,
                            Status = existingEntity2.Status,
                            Release = existingEntity2.Release,
                            Processed = existingEntity2.Processed,
                            StatusV10 = result?.DivertStatus,
                            LapCount = result?.LineCount,
                            TrackingId = trackingId,
                            Timestamp = timestampPLC,
                            CamId = Cam_Id,
                            DivertCode = 99,
                            Info = "The tote could not be diverted because it's not a 'T' Tote"
                        });
                        #endregion
                        return 99;
                    }
                }
                if (existingEntity2 != null && existingEntity2.WCS_Destination_Area == codeSingleSP && result != null)
                {
                    var timestampPLC = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    result.DivTimestamp = timestampPLC;
                    result.TrackingId = trackingId;
                    result.LastCam = Cam_Id;
                    Context.Update(result);
                    await Context.SaveChangesAsync();

                    if (Lpm.StartsWith("T"))
                    {
                        #region Scanlog
                        await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                        {
                            ToteLPN = Lpm,
                            VirtualZone = result?.ZoneDivertId,
                            VirtualTote = result?.VirtualTote,
                            Wave = existingEntity2?.Wave_Nr,
                            TotesInWave = existingEntity2?.Nr_Of_Totes_In_Wave,
                            TotalQty = existingEntity2?.Tote_Total_Qty,
                            DestinationArea = existingEntity2?.WCS_Destination_Area,
                            PutStation = existingEntity2?.Put_Station_Nr,
                            Status = existingEntity2?.Status,
                            Release = existingEntity2?.Release,
                            Processed = existingEntity2?.Processed,
                            StatusV10 = result?.DivertStatus,
                            LapCount = result?.LineCount,
                            TrackingId = trackingId,
                            Timestamp = timestampPLC,
                            CamId = Cam_Id,
                            DivertCode = 2,
                            Info = "Tote Divert Successful"
                        });
                        #endregion
                        return 2;
                    }
                    else
                    {
                        #region Scanlog
                        await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                        {
                            ToteLPN = Lpm,
                            VirtualZone = result?.ZoneDivertId,
                            VirtualTote = result?.VirtualTote,
                            Wave = existingEntity2?.Wave_Nr,
                            TotesInWave = existingEntity2?.Nr_Of_Totes_In_Wave,
                            TotalQty = existingEntity2?.Tote_Total_Qty,
                            DestinationArea = existingEntity2?.WCS_Destination_Area,
                            PutStation = existingEntity2?.Put_Station_Nr,
                            Status = existingEntity2?.Status,
                            Release = existingEntity2?.Release,
                            Processed = existingEntity2?.Processed,
                            StatusV10 = result?.DivertStatus,
                            LapCount = result?.LineCount,
                            TrackingId = trackingId,
                            Timestamp = timestampPLC,
                            CamId = Cam_Id,
                            DivertCode = 99,
                            Info = "The tote could not be diverted because it's not a 'T' Tote"
                        });
                        #endregion
                        return 99;
                    }
                }
                else
                {
                    #region Scanlog
                    await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                    {
                        ToteLPN = Lpm,
                        VirtualZone = result?.ZoneDivertId,
                        VirtualTote = result?.VirtualTote,
                        Wave = existingEntity2.Wave_Nr,
                        TotesInWave = existingEntity2.Nr_Of_Totes_In_Wave,
                        TotalQty = existingEntity2.Tote_Total_Qty,
                        DestinationArea = existingEntity2.WCS_Destination_Area,
                        PutStation = existingEntity2.Put_Station_Nr,
                        Status = existingEntity2.Status,
                        Release = existingEntity2.Release,
                        Processed = existingEntity2.Processed,
                        StatusV10 = result?.DivertStatus,
                        LapCount = result?.LineCount,
                        TrackingId = trackingId,
                        Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                        CamId = Cam_Id,
                        DivertCode = 99,
                        Info = "Tote could not be diverted, does not meet validations for single"
                    });
                    #endregion
                    return 99;
                }
            }
            catch (Exception ex)
            {
                #region Scanlog
                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                {
                    ToteLPN = Lpm,
                    TrackingId = trackingId,
                    Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    CamId = Cam_Id,
                    DivertCode = 99,
                    Info = $"Error processing tote {Lpm} in {Cam_Id}"
                });
                #endregion
                throw new Exception($"Error processing request: {ex.Message}");
            }
        }

        public async Task<int> DivertToteMulti(string Lpm, string Cam_Id, int trackingId)
        {
            try
            {
                string codeMultiSP = "SPTA";
                string codeMultiLP = "LPUT";
                string codeMultiMP = "MPAC";
                #region Querys
                #region Search General

                var result = await (from ti in Context.ToteInformations
                                    where ti.ToteLPN == Lpm && ti.DivertStatus == null
                                    select ti)
                                    .FirstOrDefaultAsync() ?? null;

                var existingEntity = await (from hdr in _e1EXtContext.ToteHdrs //<- Created for scanlogs
                                            where hdr.Tote_LPN == Lpm && hdr.Processed == null
                                            orderby hdr.Timestamp
                                            select hdr).FirstOrDefaultAsync() ?? null;
                if (existingEntity == null)
                {
                    #region Scanlog
                    await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                    {
                        ToteLPN = Lpm,
                        VirtualZone = result?.ZoneDivertId,
                        VirtualTote = result?.VirtualTote,
                        StatusV10 = result?.DivertStatus,
                        LapCount = result?.LineCount,
                        TrackingId = trackingId,
                        Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                        CamId = Cam_Id,
                        DivertCode = 99,
                        Info = "Tote not found in Tote_Hdr"
                    });
                    #endregion
                    return 99;
                }
                #endregion
                #region CheckStatusCommissioner
                bool statusCommissioner = await (from c in Context.Commissioners
                                                 where c.Status == true
                                                 select c.Status)
                                                 .FirstOrDefaultAsync();
                #endregion
                #endregion
                if (result == null)
                {
                    #region Scanlog
                    await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                    {
                        ToteLPN = Lpm,
                        VirtualZone = result?.ZoneDivertId,
                        VirtualTote = result?.VirtualTote,
                        Wave = existingEntity?.Wave_Nr,
                        TotesInWave = existingEntity?.Nr_Of_Totes_In_Wave,
                        TotalQty = existingEntity?.Tote_Total_Qty,
                        DestinationArea = existingEntity?.WCS_Destination_Area,
                        PutStation = existingEntity?.Put_Station_Nr,
                        Status = existingEntity?.Status,
                        Release = existingEntity?.Release,
                        Processed = existingEntity?.Processed,
                        StatusV10 = result?.DivertStatus,
                        LapCount = result?.LineCount,
                        TrackingId = trackingId,
                        Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                        CamId = Cam_Id,
                        DivertCode = 99,
                        Info = "Tote not registered in V10 system"
                    });
                    #endregion
                    return 99;
                }
                #region Commissioner ON
                if (statusCommissioner == true)
                {
                    if (existingEntity.WCS_Destination_Area == codeMultiLP || existingEntity.WCS_Destination_Area == codeMultiSP)
                    {
                        var timestampPLC = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        result.DivTimestamp = timestampPLC;
                        result.TrackingId = trackingId;
                        result.LastCam = Cam_Id;
                        Context.Update(result);
                        await Context.SaveChangesAsync();

                        if (existingEntity.Release >= 3)
                        {
                            #region Scanlog
                            await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                            {
                                ToteLPN = Lpm,
                                VirtualZone = result?.ZoneDivertId,
                                VirtualTote = result?.VirtualTote,
                                Wave = existingEntity?.Wave_Nr,
                                TotesInWave = existingEntity?.Nr_Of_Totes_In_Wave,
                                TotalQty = existingEntity?.Tote_Total_Qty,
                                DestinationArea = existingEntity?.WCS_Destination_Area,
                                PutStation = existingEntity?.Put_Station_Nr,
                                Status = existingEntity?.Status,
                                Release = existingEntity?.Release,
                                Processed = existingEntity?.Processed,
                                StatusV10 = result?.DivertStatus,
                                LapCount = result?.LineCount,
                                TrackingId = trackingId,
                                Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                                CamId = Cam_Id,
                                DivertCode = 2,
                                Info = "Tote relocated, correctly diverted"
                            });
                            #endregion
                            return 2;
                        }

                        #region Check Limit DiverOutboundLine
                        var limistInDivertOutPresent = await Context.DivertOutboundLines.Skip(1).FirstOrDefaultAsync();
                        #endregion

                        if (result != null)
                        {
                            if (Lpm.StartsWith("T"))
                            {
                                if (limistInDivertOutPresent.CountMultiTotes < limistInDivertOutPresent.MultiTotes)
                                {
                                    #region Check data in Shared Table
                                    var SharedData = await _e1EXtContext.SharedTable.Where(sh => sh.Wave_Nr == existingEntity.Wave_Nr).Select(sh => sh).FirstOrDefaultAsync() ?? null;
                                    #endregion
                                    if (SharedData != null)
                                    {
                                        #region Scanlog
                                        await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                                        {
                                            ToteLPN = Lpm,
                                            VirtualZone = result?.ZoneDivertId,
                                            VirtualTote = result?.VirtualTote,
                                            Wave = existingEntity?.Wave_Nr,
                                            TotesInWave = existingEntity?.Nr_Of_Totes_In_Wave,
                                            TotalQty = existingEntity?.Tote_Total_Qty,
                                            DestinationArea = existingEntity?.WCS_Destination_Area,
                                            PutStation = existingEntity?.Put_Station_Nr,
                                            Status = existingEntity?.Status,
                                            Release = existingEntity?.Release,
                                            Processed = existingEntity?.Processed,
                                            StatusV10 = result?.DivertStatus,
                                            LapCount = result?.LineCount,
                                            TrackingId = trackingId,
                                            Timestamp = timestampPLC,
                                            CamId = Cam_Id,
                                            DivertCode = 2,
                                            Info = "Tote divert successful"
                                        });
                                        #endregion
                                        return 2;
                                    }
                                }
                            }
                        }
                    }
                    #region Scanlog
                    await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                    {
                        ToteLPN = Lpm,
                        VirtualZone = result?.ZoneDivertId,
                        VirtualTote = result?.VirtualTote,
                        Wave = existingEntity?.Wave_Nr,
                        TotesInWave = existingEntity?.Nr_Of_Totes_In_Wave,
                        TotalQty = existingEntity?.Tote_Total_Qty,
                        DestinationArea = existingEntity?.WCS_Destination_Area,
                        PutStation = existingEntity?.Put_Station_Nr,
                        Status = existingEntity?.Status,
                        Release = existingEntity?.Release,
                        Processed = existingEntity?.Processed,
                        StatusV10 = result?.DivertStatus,
                        LapCount = result?.LineCount,
                        TrackingId = trackingId,
                        Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                        CamId = Cam_Id,
                        DivertCode = 99,
                        Info = "Tote not deviated, does not meet validations for multis"
                    });
                    #endregion

                    return 99;
                }
                #endregion
                #region Commissioner OFF
                if (statusCommissioner == false)
                {
                    if (existingEntity.WCS_Destination_Area == codeMultiSP || existingEntity.WCS_Destination_Area == codeMultiLP || existingEntity.WCS_Destination_Area == codeMultiMP)
                    {
                        var timestampPLC = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        result.DivTimestamp = timestampPLC;
                        result.TrackingId = trackingId;
                        result.LastCam = Cam_Id;
                        Context.Update(result);
                        await Context.SaveChangesAsync();

                        if (existingEntity.Release >= 3 && (existingEntity.Status == 4 || existingEntity.Status == 5))
                        {
                            #region Scanlog
                            await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                            {
                                ToteLPN = Lpm,
                                VirtualZone = result?.ZoneDivertId,
                                VirtualTote = result?.VirtualTote,
                                Wave = existingEntity?.Wave_Nr,
                                TotesInWave = existingEntity?.Nr_Of_Totes_In_Wave,
                                TotalQty = existingEntity?.Tote_Total_Qty,
                                DestinationArea = existingEntity?.WCS_Destination_Area,
                                PutStation = existingEntity?.Put_Station_Nr,
                                Status = existingEntity?.Status,
                                Release = existingEntity?.Release,
                                Processed = existingEntity?.Processed,
                                StatusV10 = result?.DivertStatus,
                                LapCount = result?.LineCount,
                                TrackingId = trackingId,
                                Timestamp = result?.DivTimestamp,
                                CamId = Cam_Id,
                                DivertCode = 2,
                                Info = "Tote relocated, correctly diverted"
                            });
                            #endregion

                            return 2;
                        }
                        #region Check Limit DiverOutboundLine
                        var limitsInDivertOutAbsent = await Context.DivertOutboundLines.FirstOrDefaultAsync();
                        #endregion

                        if (result != null)
                        {
                            if (Lpm.StartsWith("T"))
                            {
                                if ((existingEntity.Status == 4 || existingEntity.Status == 5)
                                    && limitsInDivertOutAbsent.CountMultiTotes < limitsInDivertOutAbsent.MultiTotes)
                                {
                                    #region Scanlog
                                    await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                                    {
                                        ToteLPN = Lpm,
                                        VirtualZone = result?.ZoneDivertId,
                                        VirtualTote = result?.VirtualTote,
                                        Wave = existingEntity?.Wave_Nr,
                                        TotesInWave = existingEntity?.Nr_Of_Totes_In_Wave,
                                        TotalQty = existingEntity?.Tote_Total_Qty,
                                        DestinationArea = existingEntity?.WCS_Destination_Area,
                                        PutStation = existingEntity?.Put_Station_Nr,
                                        Status = existingEntity?.Status,
                                        Release = existingEntity?.Release,
                                        Processed = existingEntity?.Processed,
                                        StatusV10 = result?.DivertStatus,
                                        LapCount = result?.LineCount,
                                        TrackingId = trackingId,
                                        Timestamp = timestampPLC,
                                        CamId = Cam_Id,
                                        DivertCode = 2,
                                        Info = "Tote divert successful"
                                    });
                                    #endregion

                                    return 2;
                                }
                            }
                        }
                    }                    
                    #region Scanlog
                    await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                    {
                        ToteLPN = Lpm,
                        VirtualZone = result?.ZoneDivertId,
                        VirtualTote = result?.VirtualTote,
                        Wave = existingEntity?.Wave_Nr,
                        TotesInWave = existingEntity?.Nr_Of_Totes_In_Wave,
                        TotalQty = existingEntity?.Tote_Total_Qty,
                        DestinationArea = existingEntity?.WCS_Destination_Area,
                        PutStation = existingEntity?.Put_Station_Nr,
                        Status = existingEntity?.Status,
                        Release = existingEntity?.Release,
                        Processed = existingEntity?.Processed,
                        StatusV10 = result?.DivertStatus,
                        LapCount = result?.LineCount,
                        TrackingId = trackingId,
                        Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                        CamId = Cam_Id,
                        DivertCode = 99,
                        Info = "Tote not diverted, does not meet validations for multi"
                    });
                    #endregion
                    return 99;
                }
                #endregion
                #region Scanlog
                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                {
                    ToteLPN = Lpm,
                    VirtualZone = result?.ZoneDivertId,
                    VirtualTote = result?.VirtualTote,
                    Wave = existingEntity?.Wave_Nr,
                    TotesInWave = existingEntity?.Nr_Of_Totes_In_Wave,
                    TotalQty = existingEntity?.Tote_Total_Qty,
                    DestinationArea = existingEntity?.WCS_Destination_Area,
                    PutStation = existingEntity?.Put_Station_Nr,
                    Status = existingEntity?.Status,
                    Release = existingEntity?.Release,
                    Processed = existingEntity?.Processed,
                    StatusV10 = result?.DivertStatus,
                    LapCount = result?.LineCount,
                    TrackingId = trackingId,
                    Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    CamId = Cam_Id,
                    DivertCode = 99,
                    Info = "The tote does not diverted, it does not enter any of the validations."
                });
                #endregion
                return 99;

            }
            catch (Exception ex)
            {
                #region Scanlog
                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                {
                    ToteLPN = Lpm,
                    TrackingId = trackingId,
                    Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    CamId = Cam_Id,
                    DivertCode = 99,
                    Info = $"Error processing tote {Lpm} in {Cam_Id}"
                });
                #endregion
                throw new Exception($"Error processing request: {ex.Message}");
            }
        }

        public async Task<int> DivertTote(string Lpn, string Cam_Id, int trackingId)
        {
            try
            {
                #region Query Branch UAT
                var camId = await (from c in Context.Cameras
                                   where c.CamId == Cam_Id
                                   select c.Id).FirstOrDefaultAsync();

                var divLineId4cam = await (from ca in Context.CameraAssignments
                                           where ca.CameraId == camId
                                           select ca.DivertLineId).FirstOrDefaultAsync();

                var divLineInfo = await (from dl in Context.DivertLines
                                         where dl.Id == divLineId4cam
                                         select dl).FirstOrDefaultAsync() ?? null;

                var toteInfo = await (from ti in Context.ToteInformations
                                      where ti.ToteLPN == Lpn && ti.DivertStatus == null
                                      select ti).FirstOrDefaultAsync() ?? null;

                var existingEntity = await (from hdr in _e1EXtContext.ToteHdrs //<- Created for scanlogs
                                            where hdr.Tote_LPN == Lpn && hdr.Processed == null
                                            orderby hdr.Timestamp
                                            select hdr).FirstOrDefaultAsync() ?? null;
                #endregion

                if (toteInfo != null && divLineInfo != null)
                {
                    var divLineId4Tote = await (from mp in Context.MapPhysicalVirtualSAP
                                                where mp.VirtualSAPZoneId == toteInfo.ZoneDivertId
                                                select mp.DivertLineId).FirstOrDefaultAsync();

                    var timestampPLC = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    toteInfo.DivTimestamp = timestampPLC;
                    toteInfo.TrackingId = trackingId;
                    toteInfo.LastCam = Cam_Id;
                    Context.Update(toteInfo);
                    await Context.SaveChangesAsync();

                    if (toteInfo.ZoneDivertId == 999)
                    {
                        if (await IsJackpotLine(Cam_Id))
                        {
                            #region Scanlog
                            await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                            {
                                ToteLPN = Lpn,
                                VirtualZone = toteInfo?.ZoneDivertId,
                                VirtualTote = toteInfo?.VirtualTote,
                                StatusV10 = toteInfo?.DivertStatus,
                                LapCount = toteInfo?.LineCount,
                                TrackingId = trackingId,
                                Timestamp = timestampPLC,
                                CamId = Cam_Id,
                                DivertCode = 2,
                                Info = "Tote diverted to Jackpot"
                            });
                            #endregion
                            return 2;
                        }
                    }

                    if (Lpn.StartsWith("T"))
                    {
                        if (toteInfo.ZoneDivertId == 889)
                        {
                            if (await IsPickingJackpotLine(Cam_Id))
                            {
                                #region Scanlog
                                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                                {
                                    ToteLPN = Lpn,
                                    VirtualZone = toteInfo?.ZoneDivertId,
                                    VirtualTote = toteInfo?.VirtualTote,
                                    Wave = existingEntity?.Wave_Nr,
                                    TotesInWave = existingEntity?.Nr_Of_Totes_In_Wave,
                                    TotalQty = existingEntity?.Tote_Total_Qty,
                                    DestinationArea = existingEntity?.WCS_Destination_Area,
                                    PutStation = existingEntity?.Put_Station_Nr,
                                    Status = existingEntity?.Status,
                                    Release = existingEntity?.Release,
                                    Processed = existingEntity?.Processed,
                                    StatusV10 = toteInfo?.DivertStatus,
                                    LapCount = toteInfo?.LineCount,
                                    TrackingId = trackingId,
                                    Timestamp = timestampPLC,
                                    CamId = Cam_Id,
                                    DivertCode = 2,
                                    Info = "Tote diverted to Picking Jackpot"
                                });
                                #endregion
                                return 2;
                            }
                        }
                        else
                        {
                            #region Scanlog
                            await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                            {
                                ToteLPN = Lpn,
                                VirtualZone = toteInfo?.ZoneDivertId,
                                VirtualTote = toteInfo?.VirtualTote,
                                Wave = existingEntity?.Wave_Nr,
                                TotesInWave = existingEntity?.Nr_Of_Totes_In_Wave,
                                TotalQty = existingEntity?.Tote_Total_Qty,
                                DestinationArea = existingEntity?.WCS_Destination_Area,
                                PutStation = existingEntity?.Put_Station_Nr,
                                Status = existingEntity?.Status,
                                Release = existingEntity?.Release,
                                Processed = existingEntity?.Processed,
                                StatusV10 = toteInfo?.DivertStatus,
                                LapCount = toteInfo?.LineCount,
                                TrackingId = trackingId,
                                Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                                CamId = Cam_Id,
                                DivertCode = 99,
                                Info = "Tote is Picking, it is not possible to divert"
                            });
                            #endregion
                            return 99;
                        }
                    }

                    if (divLineInfo.StatusLine == true && (divLineInfo.Id == divLineId4Tote))
                    {
                        if (Lpn.StartsWith("H") || Lpn.StartsWith("K") || Lpn.StartsWith("N"))
                        {
                            #region Scanlog
                            await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                            {
                                ToteLPN = Lpn,
                                VirtualZone = toteInfo?.ZoneDivertId,
                                VirtualTote = toteInfo?.VirtualTote,
                                StatusV10 = toteInfo?.DivertStatus,
                                LapCount = toteInfo?.LineCount,
                                TrackingId = trackingId,
                                Timestamp = timestampPLC,
                                CamId = Cam_Id,
                                DivertCode = 2,
                                Info = "Correctly diverted"
                            });
                            #endregion
                            return 2;
                        }
                    }
                }

                #region Scanlog
                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                {
                    ToteLPN = Lpn,
                    VirtualZone = toteInfo?.ZoneDivertId,
                    VirtualTote = toteInfo?.VirtualTote,
                    Wave = Lpn.StartsWith("T") ? existingEntity?.Wave_Nr : null,
                    TotesInWave = Lpn.StartsWith("T") ? existingEntity?.Nr_Of_Totes_In_Wave : null,
                    TotalQty = Lpn.StartsWith("T") ? existingEntity?.Tote_Total_Qty : null,
                    DestinationArea = Lpn.StartsWith("T") ? existingEntity?.WCS_Destination_Area : null,
                    PutStation = Lpn.StartsWith("T") ? existingEntity?.Put_Station_Nr : null,
                    Status = Lpn.StartsWith("T") ? existingEntity?.Status : null,
                    Release = Lpn.StartsWith("T") ? existingEntity?.Release : null,
                    Processed = Lpn.StartsWith("T") ? existingEntity?.Processed : null,
                    StatusV10 = toteInfo?.DivertStatus,
                    LapCount = toteInfo?.LineCount,
                    TrackingId = trackingId,
                    Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    CamId = Cam_Id,
                    DivertCode = 99,
                    Info = toteInfo == null ? "Tote not registered in V10 system" : "Wrong divert line"
                });
                #endregion
                return 99;
            }
            catch (Exception ex)
            {
                #region Scanlog
                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                {
                    ToteLPN = Lpn,
                    TrackingId = trackingId,
                    Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    CamId = Cam_Id,
                    DivertCode = 99,
                    Info = $"Error processing tote {Lpn} in {Cam_Id}"
                });
                #endregion
                throw new Exception($"Error processing request: {ex.Message}");
            }
        }

        public async Task<int> CheckTote(string LPN, int trackingId)
        {
            try
            {
                var result = await (from ti in Context.ToteInformations
                                    where ti.ToteLPN == LPN && ti.DivertStatus == null
                                    select ti).OrderBy(ti => ti.Id).LastOrDefaultAsync() ?? null;



                var divTimestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");

                string registroTipo = LPN.Substring(0, 1).ToUpper();
                if (result == null)
                {
                    //Posible error que pueda ocurrir: El LPN se encuentra en BD pero con diferente DivertLine
                    //Solucion: el LPN existe en BD > Llama a Flujo de registro que registra el mismo LPN pero con distinto VirtualTote
                    #region Scanlog
                    await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                    {
                        ToteLPN = LPN,
                        VirtualZone = result?.ZoneDivertId,
                        VirtualTote = result?.VirtualTote,
                        StatusV10 = result?.DivertStatus,
                        LapCount = result?.LineCount,
                        TrackingId = trackingId,
                        Timestamp = divTimestamp,
                        CamId = "Cam02",
                        DivertCode = 2,
                        Info = "Tote not registered in V10 system"
                    });
                    #endregion
                    return 99; //<-- Codigo para llamar al Flujo de registro
                }
                else
                {
                    //Sumamos 1 al lineCount porque el LPN ya esta registrado y no es su primer vuelta
                    result.LineCount++;
                    result.TrackingId = trackingId;
                    result.DivTimestamp = divTimestamp;
                    result.LastCam = "Cam02";
                    Context.Update(result);
                    await Context.SaveChangesAsync();

                    if (registroTipo == "T")
                    {
                        var toteHdr = await _e1EXtContext.ToteHdrs.Where(hdr => hdr.Tote_LPN == result.ToteLPN && hdr.Processed == null).OrderBy(hdr => hdr.Timestamp).FirstOrDefaultAsync() ?? null;

                        bool commissionerExists = await Context.Commissioners.AnyAsync(c => c.Status);

                        if (toteHdr == null)
                        {
                            await AssignPickingJackpotLane(LPN);
                            #region Scanlog
                            await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                            {
                                ToteLPN = LPN,
                                VirtualZone = result?.ZoneDivertId,
                                VirtualTote = result?.VirtualTote,
                                Wave = toteHdr?.Wave_Nr,
                                TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                                TotalQty = toteHdr?.Tote_Total_Qty,
                                DestinationArea = toteHdr?.WCS_Destination_Area,
                                PutStation = toteHdr?.Put_Station_Nr,
                                Status = toteHdr?.Status,
                                Release = toteHdr?.Release,
                                Processed = toteHdr?.Processed,
                                StatusV10 = result?.DivertStatus,
                                LapCount = result?.LineCount,
                                TrackingId = trackingId,
                                Timestamp = divTimestamp,
                                CamId = "Cam02",
                                DivertCode = 2,
                                Info = "Tote assigned to the picking"
                            });
                            #endregion
                            return 2;
                        }


                        if (toteHdr != null)
                        {
                            if (string.IsNullOrWhiteSpace(toteHdr.Wave_Nr))
                            {
                                await AssignPickingJackpotLane(LPN);
                                #region Scanlog
                                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                                {
                                    ToteLPN = LPN,
                                    VirtualZone = result?.ZoneDivertId,
                                    VirtualTote = result?.VirtualTote,
                                    Wave = toteHdr?.Wave_Nr,
                                    TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                                    TotalQty = toteHdr?.Tote_Total_Qty,
                                    DestinationArea = toteHdr?.WCS_Destination_Area,
                                    PutStation = toteHdr?.Put_Station_Nr,
                                    Status = toteHdr?.Status,
                                    Release = toteHdr?.Release,
                                    Processed = toteHdr?.Processed,
                                    StatusV10 = result?.DivertStatus,
                                    LapCount = result?.LineCount,
                                    TrackingId = trackingId,
                                    Timestamp = divTimestamp,
                                    CamId = "Cam02",
                                    DivertCode = 2,
                                    Info = "Tote assigned to the picking"
                                });
                                #endregion
                                return 2;
                            }

                            if ((toteHdr.WCS_Destination_Area == "SPAC" || toteHdr.WCS_Destination_Area == "SMC3") && (toteHdr.Status == null && toteHdr.Release == null))
                            {
                                await _e1EXtContext.Database.ExecuteSqlInterpolatedAsync($"UPDATE hdr SET Status = 1, Release  = 1 FROM (SELECT TOP(1) * FROM dbo.Tote_Hdr WITH (NOLOCK) WHERE Tote_LPN = {toteHdr.Tote_LPN} AND Status IS NULL AND Release IS NULL AND Processed IS NULL ORDER BY TIMESTAMP) AS hdr");
                                await _e1EXtContext.SaveChangesAsync();
                                #region Scanlog
                                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                                {
                                    ToteLPN = LPN,
                                    VirtualZone = result?.ZoneDivertId,
                                    VirtualTote = result?.VirtualTote,
                                    Wave = toteHdr?.Wave_Nr,
                                    TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                                    TotalQty = toteHdr?.Tote_Total_Qty,
                                    DestinationArea = toteHdr?.WCS_Destination_Area,
                                    PutStation = toteHdr?.Put_Station_Nr,
                                    Status = 1,
                                    Release = 1,
                                    Processed = toteHdr?.Processed,
                                    StatusV10 = result?.DivertStatus,
                                    LapCount = result?.LineCount,
                                    TrackingId = trackingId,
                                    Timestamp = divTimestamp,
                                    CamId = "Cam02",
                                    DivertCode = 2,
                                    Info = "Tote divert successful"
                                });
                                #endregion
                                return 2;
                            }

                            var waveNr = toteHdr.Wave_Nr;
                            var existingCountWaves = await Context.CountWaves.Where(cw => cw.Wave_Nr == waveNr).Select(cw => cw).FirstOrDefaultAsync() ?? null;

                            if (existingCountWaves != null)
                            {
                                var hdrCount = await _e1EXtContext.ToteHdrs.Where(hdr => hdr.Status != null && hdr.Release != null && hdr.Wave_Nr == waveNr).Select(r => r).ToListAsync();

                                #region Correccion de contador de ola
                                if (hdrCount != null && hdrCount.Count > 0)
                                {

                                    var newCount = existingCountWaves.Count == hdrCount.Count ? existingCountWaves.Count : hdrCount.Count;
                                    existingCountWaves.Count = newCount;
                                    if (existingCountWaves.Count == existingCountWaves.Nr_Of_Totes_In_Wave)
                                    {
                                        await _e1EXtContext.Database.ExecuteSqlInterpolatedAsync($"UPDATE Tote_Hdr SET Status = 2 WHERE Wave_Nr = {waveNr} AND Processed IS NULL AND Status = 1 AND Release = 1");
                                        await _e1EXtContext.SaveChangesAsync();

                                        Context.CountWaves.Remove(existingCountWaves);
                                        await Context.SaveChangesAsync();
                                        await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                                        {
                                            ToteLPN = LPN,
                                            VirtualZone = result?.ZoneDivertId,
                                            VirtualTote = result?.VirtualTote,
                                            Wave = toteHdr?.Wave_Nr,
                                            TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                                            TotalQty = toteHdr?.Tote_Total_Qty,
                                            DestinationArea = toteHdr?.WCS_Destination_Area,
                                            PutStation = toteHdr?.Put_Station_Nr,
                                            Status = 2,
                                            Release = toteHdr?.Release,
                                            Processed = toteHdr?.Processed,
                                            StatusV10 = result?.DivertStatus,
                                            LapCount = result?.LineCount,
                                            TrackingId = trackingId,
                                            Timestamp = divTimestamp,
                                            CamId = "Cam02",
                                            DivertCode = 2,
                                            Info = "Wave complete"
                                        });
                                    }
                                    await Context.SaveChangesAsync();

                                }
                                else
                                {
                                    Context.CountWaves.Remove(existingCountWaves);
                                }

                                #endregion
                            }

                            if ((toteHdr.WCS_Destination_Area == "LPUT" || toteHdr.WCS_Destination_Area == "SPTA") && (toteHdr.Status == null && toteHdr.Release == null))
                            {
                                if (toteHdr.Nr_Of_Totes_In_Wave == 1)
                                {
                                    await _e1EXtContext.Database.ExecuteSqlInterpolatedAsync($"UPDATE hdr SET Status = 2, Release  = 1 FROM (SELECT TOP(1) * FROM dbo.Tote_Hdr WITH (NOLOCK) WHERE Tote_LPN = {toteHdr.Tote_LPN} AND Status IS NULL AND Release IS NULL AND Processed IS NULL ORDER BY TIMESTAMP) AS hdr");
                                    await _e1EXtContext.SaveChangesAsync();
                                    #region Scanlog
                                    await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                                    {
                                        ToteLPN = LPN,
                                        VirtualZone = result?.ZoneDivertId,
                                        VirtualTote = result?.VirtualTote,
                                        Wave = toteHdr?.Wave_Nr,
                                        TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                                        TotalQty = toteHdr?.Tote_Total_Qty,
                                        DestinationArea = toteHdr?.WCS_Destination_Area,
                                        PutStation = toteHdr?.Put_Station_Nr,
                                        Status = 2,
                                        Release = 1,
                                        Processed = toteHdr?.Processed,
                                        StatusV10 = result?.DivertStatus,
                                        LapCount = result?.LineCount,
                                        TrackingId = trackingId,
                                        Timestamp = divTimestamp,
                                        CamId = "Cam02",
                                        DivertCode = 2,
                                        Info = "Complete wave"
                                    });
                                    #endregion
                                    return 2;
                                }
                                #region Wave creation

                                if (existingCountWaves == null)
                                {
                                    var newCountWave = new CountWave
                                    {
                                        Wave_Nr = waveNr,
                                        Nr_Of_Totes_In_Wave = toteHdr.Nr_Of_Totes_In_Wave,
                                        Count = 1
                                    };

                                    Context.CountWaves.Add(newCountWave);
                                    await Context.SaveChangesAsync();

                                    await _e1EXtContext.Database.ExecuteSqlInterpolatedAsync($"UPDATE hdr SET Status = 1, Release = 1 FROM (SELECT TOP(1) * FROM dbo.Tote_Hdr WITH (NOLOCK) WHERE Tote_LPN = {toteHdr.Tote_LPN} AND Status IS NULL AND Release IS NULL AND Processed IS NULL ORDER BY TIMESTAMP) AS hdr");
                                    await _e1EXtContext.SaveChangesAsync();

                                    var newWave = await Context.CountWaves.Where(cw => cw.Wave_Nr == waveNr).Select(cw => cw).FirstOrDefaultAsync() ?? null;
                                    var hdrCount = await _e1EXtContext.ToteHdrs.Where(hdr => hdr.Status != null && hdr.Release != null && hdr.Wave_Nr == waveNr).Select(r => r).ToListAsync();
                                    #region Scanlog
                                    await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                                    {
                                        ToteLPN = LPN,
                                        VirtualZone = result?.ZoneDivertId,
                                        VirtualTote = result?.VirtualTote,
                                        Wave = toteHdr?.Wave_Nr,
                                        TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                                        TotalQty = toteHdr?.Tote_Total_Qty,
                                        DestinationArea = toteHdr?.WCS_Destination_Area,
                                        PutStation = toteHdr?.Put_Station_Nr,
                                        Status = 1,
                                        Release = 1,
                                        Processed = toteHdr?.Processed,
                                        StatusV10 = result?.DivertStatus,
                                        LapCount = result?.LineCount,
                                        TrackingId = trackingId,
                                        Timestamp = divTimestamp,
                                        CamId = "Cam02",
                                        DivertCode = 2,
                                        Info = "Wave in process"
                                    });
                                    #endregion

                                    if (hdrCount != null && hdrCount.Count > 0)
                                    {
                                        var newCount = newWave.Count == hdrCount.Count ? newWave.Count : hdrCount.Count;

                                        newCountWave.Count = newCount;
                                        await Context.SaveChangesAsync();
                                    }

                                    if (newWave?.Count == newWave?.Nr_Of_Totes_In_Wave)
                                    {
                                        await _e1EXtContext.Database.ExecuteSqlInterpolatedAsync($"UPDATE Tote_Hdr SET Status = 2 WHERE Wave_Nr = {waveNr} AND Processed IS NULL AND Status = 1 AND Release = 1");
                                        await _e1EXtContext.SaveChangesAsync();

                                        Context.CountWaves.Remove(newWave);
                                        await Context.SaveChangesAsync();

                                        #region Scanlog
                                        await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                                        {
                                            ToteLPN = LPN,
                                            VirtualZone = result?.ZoneDivertId,
                                            VirtualTote = result?.VirtualTote,
                                            Wave = toteHdr?.Wave_Nr,
                                            TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                                            TotalQty = toteHdr?.Tote_Total_Qty,
                                            DestinationArea = toteHdr?.WCS_Destination_Area,
                                            PutStation = toteHdr?.Put_Station_Nr,
                                            Status = 2,
                                            Release = toteHdr?.Release,
                                            Processed = toteHdr?.Processed,
                                            StatusV10 = result?.DivertStatus,
                                            LapCount = result?.LineCount,
                                            TrackingId = trackingId,
                                            Timestamp = divTimestamp,
                                            CamId = "Cam02",
                                            DivertCode = 2,
                                            Info = "Wave complete"
                                        });
                                        #endregion
                                        //return 2;
                                    }

                                    return 2;
                                }
                                else
                                {
                                    if (existingCountWaves.Count < existingCountWaves.Nr_Of_Totes_In_Wave - 1)
                                    {
                                        await _e1EXtContext.Database.ExecuteSqlInterpolatedAsync($"UPDATE hdr SET Status = 1, Release  = 1 FROM (SELECT TOP(1) * FROM dbo.Tote_Hdr WITH (NOLOCK) WHERE Tote_LPN = {toteHdr.Tote_LPN} AND Status IS NULL AND Release IS NULL AND Processed IS NULL ORDER BY TIMESTAMP) AS hdr");
                                        await _e1EXtContext.SaveChangesAsync();
                                        existingCountWaves.Count++;
                                        await Context.SaveChangesAsync();
                                        #region Scanlog
                                        await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                                        {
                                            ToteLPN = LPN,
                                            VirtualZone = result?.ZoneDivertId,
                                            VirtualTote = result?.VirtualTote,
                                            Wave = toteHdr?.Wave_Nr,
                                            TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                                            TotalQty = toteHdr?.Tote_Total_Qty,
                                            DestinationArea = toteHdr?.WCS_Destination_Area,
                                            PutStation = toteHdr?.Put_Station_Nr,
                                            Status = 1,
                                            Release = 1,
                                            Processed = toteHdr?.Processed,
                                            StatusV10 = result?.DivertStatus,
                                            LapCount = result?.LineCount,
                                            TrackingId = trackingId,
                                            Timestamp = divTimestamp,
                                            CamId = "Cam02",
                                            DivertCode = 2,
                                            Info = "Wave in process"
                                        });
                                        #endregion
                                        return 2;
                                    }
                                    else
                                    {
                                        if ((existingCountWaves.Count == existingCountWaves.Nr_Of_Totes_In_Wave - 1) || (existingCountWaves.Count == existingCountWaves.Nr_Of_Totes_In_Wave))
                                        {
                                            await _e1EXtContext.Database.ExecuteSqlInterpolatedAsync($"UPDATE Tote_Hdr SET Status = 2, Release = 1 WHERE Tote_LPN = {toteHdr.Tote_LPN} AND Wave_Nr = {waveNr} AND Processed IS NULL AND (Status IS NULL OR Status = 1) AND (Release IS NULL OR Release = 1)");
                                            await _e1EXtContext.SaveChangesAsync();
                                            await _e1EXtContext.Database.ExecuteSqlInterpolatedAsync($"UPDATE Tote_Hdr SET Status = 2 WHERE Wave_Nr = {waveNr} AND Processed IS NULL AND (Status IS NULL OR Status = 1) AND (Release IS NULL OR Release >= 1 OR Release <= 3)");
                                            await _e1EXtContext.SaveChangesAsync();

                                            Context.CountWaves.Remove(existingCountWaves);
                                            await Context.SaveChangesAsync();
                                            #region Scanlog
                                            await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                                            {
                                                ToteLPN = LPN,
                                                VirtualZone = result?.ZoneDivertId,
                                                VirtualTote = result?.VirtualTote,
                                                Wave = toteHdr?.Wave_Nr,
                                                TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                                                TotalQty = toteHdr?.Tote_Total_Qty,
                                                DestinationArea = toteHdr?.WCS_Destination_Area,
                                                PutStation = toteHdr?.Put_Station_Nr,
                                                Status = 2,
                                                Release = 1,
                                                Processed = toteHdr?.Processed,
                                                StatusV10 = result?.DivertStatus,
                                                LapCount = result?.LineCount,
                                                TrackingId = trackingId,
                                                Timestamp = divTimestamp,
                                                CamId = "Cam02",
                                                DivertCode = 2,
                                                Info = "Complete wave"
                                            });
                                            #endregion
                                            return 2;
                                        }
                                    }
                                }
                                #endregion
                                //}
                            }
                        }
                    }


                    #region Scanlog
                    await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                    {
                        ToteLPN = LPN,
                        VirtualZone = result?.ZoneDivertId,
                        VirtualTote = result?.VirtualTote,
                        //Wave = toteHdr?.Wave_Nr,
                        //TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                        //TotalQty = toteHdr?.Tote_Total_Qty,
                        //DestinationArea = toteHdr?.WCS_Destination_Area,
                        //PutStation = toteHdr?.Put_Station_Nr,
                        //Status = toteHdr?.Status,
                        //Release = toteHdr?.Release,
                        //Processed = toteHdr?.Processed,
                        StatusV10 = result?.DivertStatus,
                        LapCount = result?.LineCount,
                        TrackingId = trackingId,
                        Timestamp = divTimestamp,
                        CamId = "Cam02",
                        DivertCode = 2,
                        Info = "The recirculation value has been updated."
                    });
                    #endregion
                    return 2; //<-- Codigo para confirmar que se encontró en BD y se realizó la actualizacion de LineCount
                }
            }
            catch (Exception ex)
            {
                #region Scanlog
                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                {
                    TrackingId = trackingId,
                    Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    CamId = "Cam02",
                    DivertCode = 2,
                    Info = $"Error processing tote {LPN} in Cam02"
                });
                #endregion
                throw new Exception($"Error processing request: {ex.Message}");
            }
        }

        public async Task<int> DivertConfirm(string div_timestamp, int trackingId, string camId)
        {
            try
            {
                DateTime div_timestampPLC = DateTime.ParseExact(div_timestamp, "yyyyMMddHHmmssfff", null);
                var minDivTimestampStr = div_timestampPLC.AddSeconds(-10).ToString("yyyyMMddHHmmssfff");
                var maxDivTimestampStr = div_timestampPLC.ToString("yyyyMMddHHmmssfff");

                var result = await Context.ToteInformations
                    .Where(ti => string.Compare(ti.DivTimestamp, minDivTimestampStr) >= 0 && string.Compare(ti.DivTimestamp, maxDivTimestampStr) <= 0 && ti.TrackingId == trackingId && ti.DivertStatus == null && ti.LastCam == camId)
                    .FirstOrDefaultAsync() ?? null;

                if (result != null)
                {
                    var divIdTote = await Context.MapPhysicalVirtualSAP.Where(mp => mp.VirtualSAPZoneId == result.ZoneDivertId).Select(mp => mp.DivertLineId).FirstOrDefaultAsync();
                    var camToteId = await Context.CameraAssignments.Where(ca => ca.DivertLineId == divIdTote).Select(ca => ca.CameraId).FirstOrDefaultAsync();
                    var camTote = await Context.Cameras.Where(c => c.Id == camToteId).Select(c => c.CamId).FirstOrDefaultAsync();
                    if (result.ToteLPN.StartsWith("T"))
                    {
                        if (result.ZoneDivertId == 889)
                        {
                            if (await IsPickingJackpotLine(camId))
                            {
                                result.DivertStatus = "IN";
                                result.TrackingId = 0;
                                Context.Update(result);

                                await Context.SaveChangesAsync();
                                #region Scanlog
                                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                                {
                                    ToteLPN = result?.ToteLPN,
                                    VirtualZone = result?.ZoneDivertId,
                                    VirtualTote = result?.VirtualTote,
                                    StatusV10 = result?.DivertStatus,
                                    LapCount = result?.LineCount,
                                    TrackingId = trackingId,
                                    Timestamp = div_timestamp,
                                    CamId = camId,
                                    DivertCode = 2,
                                    Info = "It is confirmed to Picking Jackpot"
                                });
                                #endregion
                                return 2;
                            }
                        }
                    }

                    var toteHdr = await _e1EXtContext.ToteHdrs.Where(th => th.Tote_LPN == result.ToteLPN && th.Status != 6 && th.Processed == null)
                        .OrderBy(th => th.Timestamp).FirstOrDefaultAsync() ?? null;

                    var limistInDivertOutPresent = await Context.DivertOutboundLines.Skip(1).FirstOrDefaultAsync();
                    var limitsInDivertOutAbsent = await Context.DivertOutboundLines.FirstOrDefaultAsync();

                    bool commissionerExists = await Context.Commissioners.AnyAsync(c => c.Status);

                    var highwayPikingLane = await Context.HighwayPikingLanes.FirstOrDefaultAsync();

                    if (toteHdr != null)
                    {
                        if (camId == "Cam13")
                        {
                            if (toteHdr.WCS_Destination_Area == "SPAC" || toteHdr.WCS_Destination_Area == "SMC3")
                            {
                                var cameraId = await Context.Cameras
                                    .Where(c => c.CamId == "Cam10")
                                    .Select(c => c.Id)
                                    .FirstOrDefaultAsync();

                                if (cameraId != 0)
                                {
                                    var limitSetting = await Context.LimitSettingCameras
                                        .Where(ls => ls.CameraId == cameraId)
                                        .FirstOrDefaultAsync();

                                    if (limitSetting != null)
                                    {
                                        if (limitSetting.CounterTote > 0)
                                        {
                                            limitSetting.CounterTote--;

                                            result.DivertStatus = "IN";
                                            result.TrackingId = 0;
                                            Context.Update(result);

                                            await Context.SaveChangesAsync();

                                            if (toteHdr.WCS_Destination_Area == "SPAC")
                                            {
                                                await _e1EXtContext.Database.ExecuteSqlInterpolatedAsync($@"UPDATE Tote_Hdr
                                                                                                        SET Status = 6, Release = 3, Processed = 1
                                                                                                        FROM Tote_Hdr AS hdr
                                                                                                        WHERE Tote_LPN = {toteHdr.Tote_LPN}
                                                                                                        AND Status <> 4
                                                                                                        AND Processed IS NULL
                                                                                                        AND hdr.TIMESTAMP = (
                                                                                                            SELECT TOP(1) hdr.TIMESTAMP
                                                                                                            FROM Tote_Hdr WITH (NOLOCK)
                                                                                                            WHERE Tote_LPN = {toteHdr.Tote_LPN}
                                                                                                            AND Status <> 4
                                                                                                            AND Processed IS NULL
                                                                                                            ORDER BY TIMESTAMP
                                                                                                        )");

                                            }
                                            else
                                            {
                                                await _e1EXtContext.Database.ExecuteSqlInterpolatedAsync($@"UPDATE Tote_Hdr
                                                                                                        SET Status = 6, Release = 3
                                                                                                        FROM Tote_Hdr AS hdr
                                                                                                        WHERE Tote_LPN = {toteHdr.Tote_LPN}
                                                                                                        AND Status <> 4
                                                                                                        AND Processed IS NULL
                                                                                                        AND hdr.TIMESTAMP = (
                                                                                                            SELECT TOP(1) hdr.TIMESTAMP
                                                                                                            FROM Tote_Hdr WITH (NOLOCK)
                                                                                                            WHERE Tote_LPN = {toteHdr.Tote_LPN}
                                                                                                            AND Status <> 4
                                                                                                            AND Processed IS NULL
                                                                                                            ORDER BY TIMESTAMP
                                                                                                        )");
                                            }

                                            await _e1EXtContext.SaveChangesAsync();

                                            #region Scanlog
                                            await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                                            {
                                                ToteLPN = result?.ToteLPN,
                                                VirtualZone = result?.ZoneDivertId,
                                                VirtualTote = result?.VirtualTote,
                                                Wave = toteHdr?.Wave_Nr,
                                                TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                                                TotalQty = toteHdr?.Tote_Total_Qty,
                                                DestinationArea = toteHdr?.WCS_Destination_Area,
                                                PutStation = toteHdr?.Put_Station_Nr,
                                                Status = toteHdr?.Status,
                                                Release = toteHdr?.Release,
                                                Processed = toteHdr?.Processed,
                                                StatusV10 = result?.DivertStatus,
                                                LapCount = result?.LineCount,
                                                TrackingId = trackingId,
                                                Timestamp = div_timestamp,
                                                CamId = camId,
                                                DivertCode = 2,
                                                Info = "The Tote has been successfully confirmed"
                                            });
                                            #endregion
                                            return 2;
                                        }
                                    }
                                }
                            }
                            #region Scanlog
                            await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                            {
                                ToteLPN = result?.ToteLPN,
                                VirtualZone = result?.ZoneDivertId,
                                VirtualTote = result?.VirtualTote,
                                Wave = toteHdr?.Wave_Nr,
                                TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                                TotalQty = toteHdr?.Tote_Total_Qty,
                                DestinationArea = toteHdr?.WCS_Destination_Area,
                                PutStation = toteHdr?.Put_Station_Nr,
                                Status = toteHdr?.Status,
                                Release = toteHdr?.Release,
                                Processed = toteHdr?.Processed,
                                StatusV10 = result?.DivertStatus,
                                LapCount = result?.LineCount,
                                TrackingId = trackingId,
                                Timestamp = div_timestamp,
                                CamId = camId,
                                DivertCode = 99,
                                Info = "Error occurred while confirming"
                            });
                            #endregion
                            return 99;
                        }

                        if (camId == "Cam12")
                        {
                            if(toteHdr.Release >= 3)
                            {
                                #region Scanlog
                                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                                {
                                    ToteLPN = result?.ToteLPN,
                                    VirtualZone = result?.ZoneDivertId,
                                    VirtualTote = result?.VirtualTote,
                                    Wave = toteHdr?.Wave_Nr,
                                    TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                                    TotalQty = toteHdr?.Tote_Total_Qty,
                                    DestinationArea = toteHdr?.WCS_Destination_Area,
                                    PutStation = toteHdr?.Put_Station_Nr,
                                    Status = toteHdr?.Status,
                                    Release = toteHdr?.Release,
                                    Processed = toteHdr?.Processed,
                                    StatusV10 = result?.DivertStatus,
                                    LapCount = result?.LineCount,
                                    TrackingId = trackingId,
                                    Timestamp = div_timestamp,
                                    CamId = camId,
                                    DivertCode = 2,
                                    Info = "The Tote has been successfully confirmed"
                                });
                                #endregion
                                return 2;
                            }

                            if (commissionerExists)
                            {
                                if (toteHdr.WCS_Destination_Area == "SPTA" || toteHdr.WCS_Destination_Area == "LPUT")
                                {
                                    if (limistInDivertOutPresent != null)
                                    {
                                        limistInDivertOutPresent.CountMultiTotes++;

                                        if (highwayPikingLane != null)
                                        {
                                            if (highwayPikingLane.CountMultiTotes > 0)
                                            {
                                                highwayPikingLane.CountMultiTotes--;

                                                await _e1EXtContext.Database.ExecuteSqlInterpolatedAsync($@"UPDATE Tote_Hdr
                                                                                                            SET Release = 3
                                                                                                            FROM Tote_Hdr AS hdr
                                                                                                            WHERE Tote_LPN = {toteHdr.Tote_LPN}
                                                                                                            AND Status < 6
                                                                                                            AND Processed IS NULL
                                                                                                            AND hdr.TIMESTAMP = (
                                                                                                                SELECT TOP(1) TIMESTAMP
                                                                                                                FROM dbo.Tote_Hdr WITH (NOLOCK)
                                                                                                                WHERE Tote_LPN = {toteHdr.Tote_LPN}
                                                                                                                AND Status < 6
                                                                                                                AND Processed IS NULL
                                                                                                                ORDER BY TIMESTAMP
                                                                                                            )");

                                                await _e1EXtContext.SaveChangesAsync();
                                                #region Scanlog
                                                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                                                {
                                                    ToteLPN = result?.ToteLPN,
                                                    VirtualZone = result?.ZoneDivertId,
                                                    VirtualTote = result?.VirtualTote,
                                                    Wave = toteHdr?.Wave_Nr,
                                                    TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                                                    TotalQty = toteHdr?.Tote_Total_Qty,
                                                    DestinationArea = toteHdr?.WCS_Destination_Area,
                                                    PutStation = toteHdr?.Put_Station_Nr,
                                                    Status = toteHdr?.Status,
                                                    Release = toteHdr?.Release,
                                                    Processed = toteHdr?.Processed,
                                                    StatusV10 = result?.DivertStatus,
                                                    LapCount = result?.LineCount,
                                                    TrackingId = trackingId,
                                                    Timestamp = div_timestamp,
                                                    CamId = camId,
                                                    DivertCode = 2,
                                                    Info = "The Tote has been successfully confirmed"
                                                });
                                                #endregion
                                                await Context.SaveChangesAsync();
                                                return 2;
                                            }
                                        }

                                    }
                                }
                                #region Scanlog
                                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                                {
                                    ToteLPN = result?.ToteLPN,
                                    VirtualZone = result?.ZoneDivertId,
                                    VirtualTote = result?.VirtualTote,
                                    Wave = toteHdr?.Wave_Nr,
                                    TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                                    TotalQty = toteHdr?.Tote_Total_Qty,
                                    DestinationArea = toteHdr?.WCS_Destination_Area,
                                    PutStation = toteHdr?.Put_Station_Nr,
                                    Status = toteHdr?.Status,
                                    Release = toteHdr?.Release,
                                    Processed = toteHdr?.Processed,
                                    StatusV10 = result?.DivertStatus,
                                    LapCount = result?.LineCount,
                                    TrackingId = trackingId,
                                    Timestamp = div_timestamp,
                                    CamId = camId,
                                    DivertCode = 99,
                                    Info = "Error occurred while confirming"
                                });
                                #endregion
                                return 99;
                            }

                            if (!commissionerExists)
                            {
                                if (toteHdr.WCS_Destination_Area == "SPTA" || toteHdr.WCS_Destination_Area == "LPUT")
                                {
                                    if (limitsInDivertOutAbsent != null)
                                    {
                                        limitsInDivertOutAbsent.CountMultiTotes++;

                                        if (highwayPikingLane != null)
                                        {
                                            if (highwayPikingLane.CountMultiTotes > 0)
                                            {
                                                highwayPikingLane.CountMultiTotes--;

                                                await _e1EXtContext.Database.ExecuteSqlInterpolatedAsync($@"UPDATE Tote_Hdr
                                                                                                            SET Release = 3
                                                                                                            FROM Tote_Hdr AS hdr
                                                                                                            WHERE Tote_LPN = {toteHdr.Tote_LPN}
                                                                                                            AND (Status = 4 OR Status = 5)
                                                                                                            AND Processed IS NULL
                                                                                                            AND hdr.TIMESTAMP = (
                                                                                                                SELECT TOP(1) TIMESTAMP
                                                                                                                FROM dbo.Tote_Hdr WITH (NOLOCK)
                                                                                                                WHERE Tote_LPN = {toteHdr.Tote_LPN}
                                                                                                                AND (Status = 4 OR Status = 5)
                                                                                                                AND Processed IS NULL
                                                                                                                ORDER BY TIMESTAMP
                                                                                                            )");

                                                await _e1EXtContext.SaveChangesAsync();
                                                #region Scanlog
                                                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                                                {
                                                    ToteLPN = result?.ToteLPN,
                                                    VirtualZone = result?.ZoneDivertId,
                                                    VirtualTote = result?.VirtualTote,
                                                    Wave = toteHdr?.Wave_Nr,
                                                    TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                                                    TotalQty = toteHdr?.Tote_Total_Qty,
                                                    DestinationArea = toteHdr?.WCS_Destination_Area,
                                                    PutStation = toteHdr?.Put_Station_Nr,
                                                    Status = toteHdr?.Status,
                                                    Release = toteHdr?.Release,
                                                    Processed = toteHdr?.Processed,
                                                    StatusV10 = result?.DivertStatus,
                                                    LapCount = result?.LineCount,
                                                    TrackingId = trackingId,
                                                    Timestamp = div_timestamp,
                                                    CamId = camId,
                                                    DivertCode = 2,
                                                    Info = "The Tote has been successfully confirmed"
                                                });
                                                #endregion
                                                await Context.SaveChangesAsync();
                                                return 2;
                                            }
                                        }
                                    }
                                }
                                #region Scanlog
                                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                                {
                                    ToteLPN = result?.ToteLPN,
                                    VirtualZone = result?.ZoneDivertId,
                                    VirtualTote = result?.VirtualTote,
                                    Wave = toteHdr?.Wave_Nr,
                                    TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                                    TotalQty = toteHdr?.Tote_Total_Qty,
                                    DestinationArea = toteHdr?.WCS_Destination_Area,
                                    PutStation = toteHdr?.Put_Station_Nr,
                                    Status = toteHdr?.Status,
                                    Release = toteHdr?.Release,
                                    Processed = toteHdr?.Processed,
                                    StatusV10 = result?.DivertStatus,
                                    LapCount = result?.LineCount,
                                    TrackingId = trackingId,
                                    Timestamp = div_timestamp,
                                    CamId = camId,
                                    DivertCode = 99,
                                    Info = "Error occurred while confirming"
                                });
                                #endregion
                                return 99;
                            }
                            #region Scanlog
                            await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                            {
                                ToteLPN = result?.ToteLPN,
                                VirtualZone = result?.ZoneDivertId,
                                VirtualTote = result?.VirtualTote,
                                Wave = toteHdr?.Wave_Nr,
                                TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                                TotalQty = toteHdr?.Tote_Total_Qty,
                                DestinationArea = toteHdr?.WCS_Destination_Area,
                                PutStation = toteHdr?.Put_Station_Nr,
                                Status = toteHdr?.Status,
                                Release = toteHdr?.Release,
                                Processed = toteHdr?.Processed,
                                StatusV10 = result?.DivertStatus,
                                LapCount = result?.LineCount,
                                TrackingId = trackingId,
                                Timestamp = div_timestamp,
                                CamId = camId,
                                DivertCode = 2,
                                Info = "Error occurred while confirming"
                            });
                            #endregion
                            return 99;
                        }

                    }

                    if (camTote != null && camTote == camId)
                    {
                        result.DivertStatus = "IN";
                        result.TrackingId = 0;
                        Context.Update(result);

                        if (result.VirtualTote.Length > 0)
                        {
                            _receivingWMSContext.Database.ExecuteSqlRaw($"INSERT INTO Wcs_ZoneRouting (Tote_ID, Virtual_Tote_ID, Zone, Insert_ts, Status) VALUES('{result.ToteLPN}','{result.VirtualTote}','{result.ZoneDivertId}', CONVERT(CHAR(23), CONVERT(DATETIME, GETDATE(), 101), 121), 'IN')");
                        }
                        await _receivingWMSContext.SaveChangesAsync();
                        await Context.SaveChangesAsync();

                        #region Scanlog
                        await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                        {
                            ToteLPN = result?.ToteLPN,
                            VirtualZone = result?.ZoneDivertId,
                            VirtualTote = result?.VirtualTote,
                            Wave = toteHdr?.Wave_Nr,
                            TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                            TotalQty = toteHdr?.Tote_Total_Qty,
                            DestinationArea = toteHdr?.WCS_Destination_Area,
                            PutStation = toteHdr?.Put_Station_Nr,
                            Status = toteHdr?.Status,
                            Release = toteHdr?.Release,
                            Processed = toteHdr?.Processed,
                            StatusV10 = result?.DivertStatus,
                            LapCount = result?.LineCount,
                            TrackingId = trackingId,
                            Timestamp = div_timestamp,
                            CamId = camId,
                            DivertCode = 2,
                            Info = "The Tote has been successfully confirmed"
                        });
                        #endregion

                        return 2;
                    }
                    else
                    {
                        #region Scanlog
                        await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                        {
                            ToteLPN = result?.ToteLPN,
                            VirtualZone = result?.ZoneDivertId,
                            VirtualTote = result?.VirtualTote,
                            Wave = toteHdr?.Wave_Nr,
                            TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                            TotalQty = toteHdr?.Tote_Total_Qty,
                            DestinationArea = toteHdr?.WCS_Destination_Area,
                            PutStation = toteHdr?.Put_Station_Nr,
                            Status = toteHdr?.Status,
                            Release = toteHdr?.Release,
                            Processed = toteHdr?.Processed,
                            StatusV10 = result?.DivertStatus,
                            LapCount = result?.LineCount,
                            TrackingId = trackingId,
                            Timestamp = div_timestamp,
                            CamId = camId,
                            DivertCode = 99,
                            Info = "Error occurred while confirming"
                        });
                        #endregion
                        return 99;
                    }
                }
                else
                {
                    #region Scanlog
                    await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                    {
                        ToteLPN = result?.ToteLPN,
                        VirtualZone = result?.ZoneDivertId,
                        VirtualTote = result?.VirtualTote,
                        StatusV10 = result?.DivertStatus,
                        LapCount = result?.LineCount,
                        TrackingId = trackingId,
                        Timestamp = div_timestamp,
                        CamId = camId,
                        DivertCode = 99,
                        Info = "Error occurred while confirming. Tote not found in time range"
                    });
                    #endregion
                    return 99;
                }
            }
            catch (Exception ex)
            {
                #region Scanlog
                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                {
                    TrackingId = trackingId,
                    Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    CamId = camId,
                    DivertCode = 2,
                    Info = $"Error processing tote with TrackingId {trackingId} in {camId}"
                });
                #endregion
                throw new Exception($"Error processing request: {ex.Message}");
            }
        }

        public async Task<int> DivertConfirmCam14(string div_timestamp, int trackingId, string camId)
        {
            try
            {
                DateTime div_timestampPLC = DateTime.ParseExact(div_timestamp, "yyyyMMddHHmmssfff", null);
                var minDivTimestampStr = div_timestampPLC.AddSeconds(-10).ToString("yyyyMMddHHmmssfff");
                var maxDivTimestampStr = div_timestampPLC.ToString("yyyyMMddHHmmssfff");

                var result = await Context.ToteInformations
                    .Where(ti => string.Compare(ti.DivTimestamp, minDivTimestampStr) >= 0 && string.Compare(ti.DivTimestamp, maxDivTimestampStr) <= 0 && ti.TrackingId == trackingId && ti.DivertStatus == null && ti.LastCam == camId)
                    .FirstOrDefaultAsync() ?? null;


                if (result != null)
                {
                    var toteHdr = await _e1EXtContext.ToteHdrs
                                       .Where(th => th.Tote_LPN == result.ToteLPN && th.Status != 6 && th.Release <= 4 && th.Processed == null).OrderBy(th => th.Timestamp).FirstOrDefaultAsync() ?? null;

                    var limistInDivertOutPresent = await Context.DivertOutboundLines.Skip(1).FirstOrDefaultAsync();

                    bool commissionerExists = await Context.Commissioners.AnyAsync(c => c.Status);

                    if (toteHdr != null)
                    {
                        if ("Cam14" == camId)
                        {
                            if (commissionerExists)
                            {
                                if (limistInDivertOutPresent != null && toteHdr.Release < 4)
                                {
                                    if (limistInDivertOutPresent.CountMultiTotes > 0)
                                    {
                                        limistInDivertOutPresent.CountMultiTotes--;
                                        await Context.SaveChangesAsync();
                                    }
                                }

                                await _e1EXtContext.Database.ExecuteSqlInterpolatedAsync($@"UPDATE Tote_Hdr
                                                                                                SET Release = 4
                                                                                                FROM Tote_Hdr AS hdr
                                                                                                WHERE Tote_LPN = {toteHdr.Tote_LPN}
                                                                                                AND Status <> 6
                                                                                                AND Release < 4
                                                                                                AND Processed IS NULL
                                                                                                AND hdr.TIMESTAMP = (
                                                                                                    SELECT TOP(1) TIMESTAMP
                                                                                                    FROM dbo.Tote_Hdr WITH (NOLOCK)
                                                                                                    WHERE Tote_LPN = {toteHdr.Tote_LPN}
                                                                                                    AND Status <> 6
                                                                                                    AND Release < 4
                                                                                                    AND Processed IS NULL
                                                                                                    ORDER BY TIMESTAMP
                                                                                                )");

                                await _e1EXtContext.SaveChangesAsync();

                                #region Scanlog
                                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                                {
                                    ToteLPN = result?.ToteLPN,
                                    VirtualZone = result?.ZoneDivertId,
                                    VirtualTote = result?.VirtualTote,
                                    Wave = toteHdr?.Wave_Nr,
                                    TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                                    TotalQty = toteHdr?.Tote_Total_Qty,
                                    DestinationArea = toteHdr?.WCS_Destination_Area,
                                    PutStation = toteHdr?.Put_Station_Nr,
                                    Status = toteHdr?.Status,
                                    Release = toteHdr?.Release,
                                    Processed = toteHdr?.Processed,
                                    StatusV10 = result?.DivertStatus,
                                    LapCount = result?.LineCount,
                                    TrackingId = trackingId,
                                    Timestamp = div_timestamp,
                                    CamId = camId,
                                    DivertCode = 2,
                                    Info = "The Tote has been successfully confirmed"
                                });
                                #endregion
                                return 2;
                            }
                        }
                    }
                }
                #region Scanlog
                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                {
                    ToteLPN = result?.ToteLPN,
                    VirtualZone = result?.ZoneDivertId,
                    VirtualTote = result?.VirtualTote,
                    StatusV10 = result?.DivertStatus,
                    LapCount = result?.LineCount,
                    TrackingId = trackingId,
                    Timestamp = div_timestamp,
                    CamId = camId,
                    DivertCode = 99,
                    Info = "Error occurred while confirming"
                });
                #endregion
                return 99;
            }
            catch (Exception ex)
            {
                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                {
                    TrackingId = trackingId,
                    Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    CamId = camId,
                    DivertCode = 2,
                    Info = $"Error processing tote with TrackingId {trackingId} in {camId}"
                });
                //await updateScanLog(null, camId, 99);
                throw new Exception($"Error processing request: {ex.Message}");
            }
        }

        public async Task<int> DivertConfirmCam15(string div_timestamp, int trackingId, string camId)
        {
            try
            {
                DateTime div_timestampPLC = DateTime.ParseExact(div_timestamp, "yyyyMMddHHmmssfff", null);
                var minDivTimestampStr = div_timestampPLC.AddSeconds(-10).ToString("yyyyMMddHHmmssfff");
                var maxDivTimestampStr = div_timestampPLC.ToString("yyyyMMddHHmmssfff");

                var result = await Context.ToteInformations
                    .Where(ti => string.Compare(ti.DivTimestamp, minDivTimestampStr) >= 0 && string.Compare(ti.DivTimestamp, maxDivTimestampStr) <= 0 && ti.TrackingId == trackingId && ti.DivertStatus == null && ti.LastCam == camId)
                    .FirstOrDefaultAsync() ?? null;



                if (result != null)
                {
                    var limistInDivertOutPresent = await Context.DivertOutboundLines.Skip(1).FirstOrDefaultAsync();
                    var limitsInDivertOutAbsent = await Context.DivertOutboundLines.FirstOrDefaultAsync();

                    bool commissionerExists = await Context.Commissioners.AnyAsync(c => c.Status);

                    var toteHdr = await _e1EXtContext.ToteHdrs
                                        .Where(th => th.Tote_LPN == result.ToteLPN && (th.Status == 4 || th.Status == 5) && th.Processed == null).OrderBy(th => th.Timestamp).FirstOrDefaultAsync() ?? null;


                    if (toteHdr != null)
                    {
                        if ("Cam15" == camId)
                        {
                            if (toteHdr.WCS_Destination_Area == "LPUT" && (toteHdr.Put_Station_Nr == 1 || toteHdr.Put_Station_Nr == 2))
                            {
                                if (commissionerExists)
                                {
                                    if (limistInDivertOutPresent != null)
                                    {
                                        if (limistInDivertOutPresent.CountMultiTotes > 0)
                                        {
                                            limistInDivertOutPresent.CountMultiTotes--;
                                            await Context.SaveChangesAsync();

                                        }
                                    }
                                }else if (!commissionerExists)
                                {
                                    if (limitsInDivertOutAbsent != null)
                                    {
                                        if (limitsInDivertOutAbsent.CountMultiTotes > 0)
                                        {
                                            limitsInDivertOutAbsent.CountMultiTotes--;
                                            await Context.SaveChangesAsync();
                                        }
                                    }
                                  
                                }

                                result.DivertStatus = "IN";
                                result.TrackingId = 0;
                                Context.Update(result);
                                await Context.SaveChangesAsync();

                                await _e1EXtContext.Database.ExecuteSqlInterpolatedAsync($@"UPDATE Tote_Hdr
                                                                                                    SET Release = 15
                                                                                                    FROM Tote_Hdr AS hdr
                                                                                                    WHERE Tote_LPN = {toteHdr.Tote_LPN}
                                                                                                    AND (Status = 4 OR Status = 5)
                                                                                                    AND Processed IS NULL
                                                                                                    AND hdr.TIMESTAMP = (
                                                                                                        SELECT TOP(1) TIMESTAMP
                                                                                                        FROM dbo.Tote_Hdr WITH (NOLOCK)
                                                                                                        WHERE Tote_LPN = {toteHdr.Tote_LPN}
                                                                                                        AND (Status = 4 OR Status = 5)
                                                                                                        AND Processed IS NULL
                                                                                                        ORDER BY TIMESTAMP
                                                                                                    )");
                                await _e1EXtContext.SaveChangesAsync();

                                #region Scanlog
                                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                                {
                                    ToteLPN = result?.ToteLPN,
                                    VirtualZone = result?.ZoneDivertId,
                                    VirtualTote = result?.VirtualTote,
                                    Wave = toteHdr?.Wave_Nr,
                                    TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                                    TotalQty = toteHdr?.Tote_Total_Qty,
                                    DestinationArea = toteHdr?.WCS_Destination_Area,
                                    PutStation = toteHdr?.Put_Station_Nr,
                                    Status = toteHdr?.Status,
                                    Release = toteHdr?.Release,
                                    Processed = toteHdr?.Processed,
                                    StatusV10 = result?.DivertStatus,
                                    LapCount = result?.LineCount,
                                    TrackingId = trackingId,
                                    Timestamp = div_timestamp,
                                    CamId = camId,
                                    DivertCode = 2,
                                    Info = "The Tote has been successfully confirmed"
                                });
                                #endregion
                                return 2;
                            }
                        }
                    }
                }
                #region Scanlog
                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                {
                    ToteLPN = result?.ToteLPN,
                    VirtualZone = result?.ZoneDivertId,
                    VirtualTote = result?.VirtualTote,
                    StatusV10 = result?.DivertStatus,
                    LapCount = result?.LineCount,
                    TrackingId = trackingId,
                    Timestamp = div_timestamp,
                    CamId = camId,
                    DivertCode = 99,
                    Info = "Error occurred while confirming"
                });
                #endregion
                return 99;
            }
            catch (Exception ex)
            {
                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                {
                    TrackingId = trackingId,
                    Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    CamId = camId,
                    DivertCode = 2,
                    Info = $"Error processing tote with TrackingId {trackingId} in {camId}"
                });
                throw new Exception($"Error processing request: {ex.Message}");
            }
        }

        public async Task<int> DivertConfirmCam16(string div_timestamp, int trackingId, string camId)
        {
            try
            {
                DateTime div_timestampPLC = DateTime.ParseExact(div_timestamp, "yyyyMMddHHmmssfff", null);
                var minDivTimestampStr = div_timestampPLC.AddSeconds(-10).ToString("yyyyMMddHHmmssfff");
                var maxDivTimestampStr = div_timestampPLC.ToString("yyyyMMddHHmmssfff");

                var result = await Context.ToteInformations
                    .Where(ti => string.Compare(ti.DivTimestamp, minDivTimestampStr) >= 0 && string.Compare(ti.DivTimestamp, maxDivTimestampStr) <= 0 && ti.TrackingId == trackingId && ti.DivertStatus == null && ti.LastCam == camId)
                    .FirstOrDefaultAsync() ?? null;

                if (result != null)
                {
                    var toteHdr = await _e1EXtContext.ToteHdrs
                        .Where(th => th.Tote_LPN == result.ToteLPN && (th.Status == 4 || th.Status == 5) && th.Processed == null).OrderBy(th => th.Timestamp).FirstOrDefaultAsync() ?? null;

                    var limistInDivertOutPresent = await Context.DivertOutboundLines.Skip(1).FirstOrDefaultAsync();
                    var limitsInDivertOutAbsent = await Context.DivertOutboundLines.FirstOrDefaultAsync();

                    bool commissionerExists = await Context.Commissioners.AnyAsync(c => c.Status);

                    if (toteHdr != null)
                    {
                        if ("Cam16" == camId)
                        {
                            if (toteHdr.WCS_Destination_Area == "SPTA" && toteHdr.Put_Station_Nr == 62)
                            {
                                if (commissionerExists)
                                {
                                    if (limistInDivertOutPresent != null)
                                    {
                                        if (limistInDivertOutPresent.CountMultiTotes > 0)
                                        {
                                            limistInDivertOutPresent.CountMultiTotes--;
                                            await Context.SaveChangesAsync();
                                        }
                                    }
                                }else if (!commissionerExists)
                                {
                                    if (limitsInDivertOutAbsent != null)
                                    {
                                        if (limitsInDivertOutAbsent.CountMultiTotes > 0)
                                        {
                                            limitsInDivertOutAbsent.CountMultiTotes--;
                                            await Context.SaveChangesAsync();
                                        }
                                    }
                                }

                                result.DivertStatus = "IN";
                                result.TrackingId = 0;
                                Context.Update(result);
                                await Context.SaveChangesAsync();

                                await _e1EXtContext.Database.ExecuteSqlInterpolatedAsync($@"UPDATE Tote_Hdr
                                                                                                    SET Release = 16
                                                                                                    FROM Tote_Hdr AS hdr
                                                                                                    WHERE Tote_LPN = {toteHdr.Tote_LPN}
                                                                                                    AND (Status = 4 OR Status = 5)
                                                                                                    AND Processed IS NULL
                                                                                                    AND hdr.TIMESTAMP = (
                                                                                                        SELECT TOP(1) TIMESTAMP
                                                                                                        FROM dbo.Tote_Hdr WITH (NOLOCK)
                                                                                                        WHERE Tote_LPN = {toteHdr.Tote_LPN}
                                                                                                        AND (Status = 4 OR Status = 5)
                                                                                                        AND Processed IS NULL
                                                                                                        ORDER BY TIMESTAMP
                                                                                                    )");
                                await _e1EXtContext.SaveChangesAsync();

                                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                                {
                                    ToteLPN = toteHdr?.Tote_LPN,
                                    VirtualZone = result?.ZoneDivertId,
                                    VirtualTote = result?.VirtualTote,
                                    Wave = toteHdr.Wave_Nr,
                                    TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                                    TotalQty = toteHdr?.Tote_Total_Qty,
                                    DestinationArea = toteHdr?.WCS_Destination_Area,
                                    PutStation = toteHdr?.Put_Station_Nr,
                                    Status = toteHdr?.Status,
                                    Release = toteHdr?.Release,
                                    Processed = toteHdr?.Processed,
                                    StatusV10 = result?.DivertStatus,
                                    LapCount = result?.LineCount,
                                    TrackingId = trackingId,
                                    Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                                    CamId = camId,
                                    DivertCode = 2,
                                    Info = "The Tote has been successfully confirmed"
                                });
                                return 2;
                            }
                        }
                    }
                }
                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                {
                    TrackingId = trackingId,
                    Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    CamId = camId,
                    DivertCode = 99,
                    Info = "The Tote could not be confirmed because it does not exist in the system."
                });
                return 99;

            }
            catch (Exception ex)
            {
                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                {
                    TrackingId = trackingId,
                    Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    CamId = camId,
                    DivertCode = 99,
                    Info = $"Error confirming Tote in CAM16."
                });
                throw new Exception($"Error processing request: {ex.Message}");
            }
        }

        public async Task<int> DivertConfirmCam17(string div_timestamp, int trackingId, string camId)
        {
            try
            {
                DateTime div_timestampPLC = DateTime.ParseExact(div_timestamp, "yyyyMMddHHmmssfff", null);
                var minDivTimestampStr = div_timestampPLC.AddSeconds(-10).ToString("yyyyMMddHHmmssfff");
                var maxDivTimestampStr = div_timestampPLC.ToString("yyyyMMddHHmmssfff");

                var result = await Context.ToteInformations
                    .Where(ti => string.Compare(ti.DivTimestamp, minDivTimestampStr) >= 0 && string.Compare(ti.DivTimestamp, maxDivTimestampStr) <= 0 && ti.TrackingId == trackingId && ti.DivertStatus == null && ti.LastCam == camId)
                    .FirstOrDefaultAsync() ?? null;

                if (result != null)
                {
                    var toteHdr = await _e1EXtContext.ToteHdrs
                        .Where(th => th.Tote_LPN == result.ToteLPN && (th.Status == 4 || th.Status == 5) && th.Processed == null).OrderBy(th => th.Timestamp).FirstOrDefaultAsync() ?? null;

                    var limistInDivertOutPresent = await Context.DivertOutboundLines.Skip(1).FirstOrDefaultAsync();
                    var limitsInDivertOutAbsent = await Context.DivertOutboundLines.FirstOrDefaultAsync();

                    bool commissionerExists = await Context.Commissioners.AnyAsync(c => c.Status);

                    if (toteHdr != null)
                    {
                        if ("Cam17" == camId)
                        {
                            if (toteHdr.WCS_Destination_Area == "SPTA" && toteHdr.Put_Station_Nr == 61)
                            {
                                if (commissionerExists)
                                {
                                    if (limistInDivertOutPresent != null)
                                    {
                                        if (limistInDivertOutPresent.CountMultiTotes > 0)
                                        {
                                            limistInDivertOutPresent.CountMultiTotes--;
                                            await Context.SaveChangesAsync();
                                        }
                                    }
                                }else if (!commissionerExists)
                                {
                                    if (limitsInDivertOutAbsent != null)
                                    {
                                        if (limitsInDivertOutAbsent.CountMultiTotes > 0)
                                        {
                                            limitsInDivertOutAbsent.CountMultiTotes--;
                                            await Context.SaveChangesAsync();
                                        }
                                    }
                                }

                                result.DivertStatus = "IN";
                                result.TrackingId = 0;
                                Context.Update(result);
                                await Context.SaveChangesAsync();

                                await _e1EXtContext.Database.ExecuteSqlInterpolatedAsync($@"UPDATE Tote_Hdr
                                                                                                    SET Release = 17
                                                                                                    FROM Tote_Hdr AS hdr
                                                                                                    WHERE Tote_LPN = {toteHdr.Tote_LPN}
                                                                                                    AND (Status = 4 OR Status = 5)
                                                                                                    AND Processed IS NULL
                                                                                                    AND hdr.TIMESTAMP = (
                                                                                                        SELECT TOP(1) TIMESTAMP
                                                                                                        FROM dbo.Tote_Hdr WITH (NOLOCK)
                                                                                                        WHERE Tote_LPN = {toteHdr.Tote_LPN}
                                                                                                        AND (Status = 4 OR Status = 5)
                                                                                                        AND Processed IS NULL
                                                                                                        ORDER BY TIMESTAMP
                                                                                                    )");
                                await _e1EXtContext.SaveChangesAsync();

                                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                                {
                                    ToteLPN = toteHdr?.Tote_LPN,
                                    VirtualZone = result?.ZoneDivertId,
                                    VirtualTote = result?.VirtualTote,
                                    Wave = toteHdr?.Wave_Nr,
                                    TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                                    TotalQty = toteHdr?.Tote_Total_Qty,
                                    DestinationArea = toteHdr?.WCS_Destination_Area,
                                    PutStation = toteHdr?.Put_Station_Nr,
                                    Status = toteHdr?.Status,
                                    Release = toteHdr?.Release,
                                    Processed = toteHdr?.Processed,
                                    StatusV10 = result?.DivertStatus,
                                    LapCount = result?.LineCount,
                                    TrackingId = trackingId,
                                    Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                                    CamId = camId,
                                    DivertCode = 2,
                                    Info = "The Tote has been successfully confirmed"
                                });
                                return 2;
                            }
                        }
                    }
                }
                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                {
                    TrackingId = trackingId,
                    Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    CamId = camId,
                    DivertCode = 99,
                    Info = "The Tote could not be confirmed because it does not exist in the system."
                });
                return 99;

            }
            catch (Exception ex)
            {
                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                {
                    TrackingId = trackingId,
                    Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    CamId = camId,
                    DivertCode = 99,
                    Info = $"Error confirming Tote in CAM17."
                });
                throw new Exception($"Error processing request: {ex.Message}");
            }
        }

        private async Task AssignPickingJackpotLane(string toteLPN)
        {
            try
            {
                int virtualSAPZoneId = 889;

                // Actualiza la información en ToteInformation
                var toteInformationList = await Context.ToteInformations
                    .Where(t => t.ToteLPN == toteLPN && t.DivertStatus != "IN")
                    .ToListAsync();

                if (toteInformationList.Count > 0)
                {
                    var lastToteInformation = toteInformationList.Last();
                    lastToteInformation.ZoneDivertId = virtualSAPZoneId;

                    await Context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error when assigning Picking Jackpot Lane.");
            }
        }

        public async Task<int> CheckLimit(string Lpm, string idCam, int trackingId)
        {
            try
            {
                #region variables for Single tote
                string codeSingleSM = "SMC3";
                string codeSingleSP = "SPAC";
                string cam10 = "Cam10";
                #endregion
                #region variables for mulit tote
                string codeMultiSP = "SPTA";
                string codeMultiLP = "LPUT";
                string codeMultiMP = "MPAC";
                int status = 3; int status2 = 4;
                #endregion
                #region Querys
                #region SarchData
                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");

                var camId = await (from c in Context.Cameras
                                   where c.CamId == idCam
                                   select c.Id)
                   .FirstOrDefaultAsync();

                var result = await (from ti in Context.ToteInformations
                                    where ti.ToteLPN == Lpm && ti.DivertStatus == null
                                    select ti)
                                    .FirstOrDefaultAsync() ?? null;

                var existingEntity = await (from hdr in _e1EXtContext.ToteHdrs //<- Created for scanlogs
                                            where hdr.Tote_LPN == Lpm && hdr.Processed == null
                                            orderby hdr.Timestamp
                                            select hdr).FirstOrDefaultAsync() ?? null;
                #endregion
                
                #region Check Limit Camera 10
                var limitTote = await (from l in Context.LimitSettingCameras
                                       where l.Camera.CamId == cam10
                                       select l.MaximumCapacity)
                                       .FirstOrDefaultAsync();

                var toteCounter = await (from l in Context.LimitSettingCameras
                                         where l.Camera.CamId == cam10
                                         select l.CounterTote)
                                       .FirstOrDefaultAsync();
                #endregion               
                #region Check Limit HighPickinglane
                var limitsInHighwayPresent = await Context.HighwayPikingLanes.FirstOrDefaultAsync();
                #endregion
                #region CheckStatusCommissioner
                bool statusCommissioner = await (from c in Context.Commissioners
                                                 where c.Status == true
                                                 select c.Status)
                                                 .FirstOrDefaultAsync();
                #endregion                
                #region waveCountList
                if (existingEntity == null)
                {
                    #region Scanlog
                    await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                    {
                        ToteLPN = Lpm,
                        VirtualZone = result?.ZoneDivertId,
                        VirtualTote = result?.VirtualTote,
                        StatusV10 = result?.DivertStatus,
                        LapCount = result?.LineCount,
                        TrackingId = trackingId,
                        Timestamp = timestamp,
                        CamId = idCam,
                        DivertCode = 1,
                        Info = "Tote not found in Tote_Hdr"
                    });
                    #endregion
                    return 2;
                }

                var listWaveComplete = await (from list in _e1EXtContext.ToteHdrs
                                              where list.Nr_Of_Totes_In_Wave == existingEntity.Nr_Of_Totes_In_Wave && list.Wave_Nr == existingEntity.Wave_Nr && (list.Status >= status)
                                              select list).ToListAsync() ?? null;
                #endregion
                
                #endregion
                #region toteResult
                if (result == null)
                {
                    #region Scanlog
                    await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                    {
                        ToteLPN = Lpm,
                        VirtualZone = result?.ZoneDivertId,
                        VirtualTote = result?.VirtualTote,
                        StatusV10 = result?.DivertStatus,
                        LapCount = result?.LineCount,
                        TrackingId = trackingId,
                        Timestamp = timestamp,
                        CamId = idCam,
                        DivertCode = 1,
                        Info = "Tote not registered in V10 system"
                    });
                    #endregion
                    return 2;
                }
                #endregion

                #region add last camera in tote information
                result.LastCam = "Cam10";
                Context.Update(result);
                await Context.SaveChangesAsync();
                #endregion

                #region Evaluation for SingelTote
                if (idCam == cam10 && existingEntity.WCS_Destination_Area == codeSingleSM || idCam == cam10 && existingEntity.WCS_Destination_Area == codeSingleSP)
                {
                    if (Lpm.StartsWith("T"))
                    {
                        if (toteCounter >= limitTote)
                        {
                            #region Scanlog
                            await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                            {
                                ToteLPN = Lpm,
                                VirtualZone = result?.ZoneDivertId,
                                VirtualTote = result?.VirtualTote,
                                Wave = existingEntity?.Wave_Nr,
                                TotesInWave = existingEntity?.Nr_Of_Totes_In_Wave,
                                TotalQty = existingEntity?.Tote_Total_Qty,
                                DestinationArea = existingEntity?.WCS_Destination_Area,
                                PutStation = existingEntity?.Put_Station_Nr,
                                Status = existingEntity?.Status,
                                Release = existingEntity?.Release,
                                Processed = existingEntity?.Processed,
                                StatusV10 = result?.DivertStatus,
                                LapCount = result?.LineCount,
                                TrackingId = trackingId,
                                Timestamp = timestamp,
                                CamId = idCam,
                                DivertCode = 1,
                                Info = "Totes singles at limited capacity"
                            });
                            #endregion
                            return 2;
                        }
                        else if (existingEntity.Status == 1)
                        {
                            await _e1EXtContext.Database.ExecuteSqlInterpolatedAsync($"UPDATE Tote_Hdr SET Release = 2 FROM Tote_Hdr AS hdr WHERE Tote_LPN = {existingEntity.Tote_LPN} AND Status <> 6 AND Processed IS NULL AND hdr.TIMESTAMP = (SELECT TOP(1) hdr.TIMESTAMP FROM Tote_Hdr WITH (NOLOCK) WHERE Tote_LPN = {existingEntity.Tote_LPN} AND Status <> 6 AND Processed IS NULL ORDER BY TIMESTAMP)");
                            await _e1EXtContext.SaveChangesAsync();
                            var valueSinlgeToteCounter = await Context.LimitSettingCameras.FirstOrDefaultAsync(x => x.CameraId == camId);
                            valueSinlgeToteCounter.CounterTote = toteCounter + 1;
                            await Context.SaveChangesAsync();

                            #region Scanlog
                            await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                            {
                                ToteLPN = Lpm,
                                VirtualZone = result?.ZoneDivertId,
                                VirtualTote = result?.VirtualTote,
                                Wave = existingEntity?.Wave_Nr,
                                TotesInWave = existingEntity?.Nr_Of_Totes_In_Wave,
                                TotalQty = existingEntity?.Tote_Total_Qty,
                                DestinationArea = existingEntity?.WCS_Destination_Area,
                                PutStation = existingEntity?.Put_Station_Nr,
                                Status = existingEntity?.Status,
                                Release = 2,
                                Processed = existingEntity?.Processed,
                                StatusV10 = result?.DivertStatus,
                                LapCount = result?.LineCount,
                                TrackingId = trackingId,
                                Timestamp = timestamp,
                                CamId = idCam,
                                DivertCode = 99,
                                Info = "Tote single correctly diverted"
                            });
                            #endregion

                            return 99;
                        }
                    }
                }
                #endregion

                #region Evaluation for MultiTotes
                #region Commissiner is true
                if (statusCommissioner == true)
                {
                    if (existingEntity.Release >= 2)
                    {
                        #region Scanlog
                        await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                        {
                            ToteLPN = Lpm,
                            VirtualZone = result?.ZoneDivertId,
                            VirtualTote = result?.VirtualTote,
                            Wave = existingEntity?.Wave_Nr,
                            TotesInWave = existingEntity?.Nr_Of_Totes_In_Wave,
                            TotalQty = existingEntity?.Tote_Total_Qty,
                            DestinationArea = existingEntity?.WCS_Destination_Area,
                            PutStation = existingEntity?.Put_Station_Nr,
                            Status = existingEntity?.Status,
                            Release = existingEntity?.Release,
                            Processed = existingEntity?.Processed,
                            StatusV10 = result?.DivertStatus,
                            LapCount = result?.LineCount,
                            TrackingId = trackingId,
                            Timestamp = timestamp,
                            CamId = idCam,
                            DivertCode = 99,
                            Info = "Tote reassigned to the last position"
                        });
                        #endregion
                        return 99;
                    }
                    if (idCam == cam10 && (existingEntity.WCS_Destination_Area == codeMultiSP || existingEntity.WCS_Destination_Area == codeMultiLP || existingEntity.WCS_Destination_Area == codeMultiMP) && existingEntity.Status >= 1)
                    {
                        if (Lpm.StartsWith("T"))
                        {
                            if (limitsInHighwayPresent.CountMultiTotes < limitsInHighwayPresent.MultiTotes)
                            {
                                #region Check data in Shared Table
                                var SharedData = await _e1EXtContext.SharedTable.Where(sh => sh.Wave_Nr == existingEntity.Wave_Nr).Select(sh => sh).FirstOrDefaultAsync() ?? null;
                                #endregion
                                if (SharedData != null)
                                {
                                    await _e1EXtContext.Database.ExecuteSqlInterpolatedAsync($"UPDATE Tote_Hdr SET Release = 2 FROM Tote_Hdr AS hdr WHERE Tote_LPN = {existingEntity.Tote_LPN} AND Status <> 6 AND Processed IS NULL AND hdr.TIMESTAMP = (SELECT TOP(1) hdr.TIMESTAMP FROM Tote_Hdr WITH (NOLOCK) WHERE Tote_LPN = {existingEntity.Tote_LPN} AND Status <> 6 AND Processed IS NULL ORDER BY TIMESTAMP)");
                                    await _e1EXtContext.SaveChangesAsync();
                                    limitsInHighwayPresent.CountMultiTotes = limitsInHighwayPresent.CountMultiTotes + 1;
                                    await Context.SaveChangesAsync();

                                    #region Scanlog
                                    await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                                    {
                                        ToteLPN = Lpm,
                                        VirtualZone = result?.ZoneDivertId,
                                        VirtualTote = result?.VirtualTote,
                                        Wave = existingEntity?.Wave_Nr,
                                        TotesInWave = existingEntity?.Nr_Of_Totes_In_Wave,
                                        TotalQty = existingEntity?.Tote_Total_Qty,
                                        DestinationArea = existingEntity?.WCS_Destination_Area,
                                        PutStation = existingEntity?.Put_Station_Nr,
                                        Status = existingEntity?.Status,
                                        Release = 2,
                                        Processed = existingEntity?.Processed,
                                        StatusV10 = result?.DivertStatus,
                                        LapCount = result?.LineCount,
                                        TrackingId = trackingId,
                                        Timestamp = timestamp,
                                        CamId = idCam,
                                        DivertCode = 99,
                                        Info = "Tote multi correctly diverted"
                                    });
                                    #endregion
                                    return 99;
                                }
                            }
                            else
                            {
                                #region Scanlog
                                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                                {
                                    ToteLPN = Lpm,
                                    VirtualZone = result?.ZoneDivertId,
                                    VirtualTote = result?.VirtualTote,
                                    Wave = existingEntity?.Wave_Nr,
                                    TotesInWave = existingEntity?.Nr_Of_Totes_In_Wave,
                                    TotalQty = existingEntity?.Tote_Total_Qty,
                                    DestinationArea = existingEntity?.WCS_Destination_Area,
                                    PutStation = existingEntity?.Put_Station_Nr,
                                    Status = existingEntity?.Status,
                                    Release = existingEntity?.Release,
                                    Processed = existingEntity?.Processed,
                                    StatusV10 = result?.DivertStatus,
                                    LapCount = result?.LineCount,
                                    TrackingId = trackingId,
                                    Timestamp = timestamp,
                                    CamId = idCam,
                                    DivertCode = 1,
                                    Info = "Totes multis at limited capacity"
                                });
                                #endregion
                                return 2;
                            }
                        }
                    }

                    #region Scanlog
                    await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                    {
                        ToteLPN = Lpm,
                        VirtualZone = result?.ZoneDivertId,
                        VirtualTote = result?.VirtualTote,
                        Wave = existingEntity?.Wave_Nr,
                        TotesInWave = existingEntity?.Nr_Of_Totes_In_Wave,
                        TotalQty = existingEntity?.Tote_Total_Qty,
                        DestinationArea = existingEntity?.WCS_Destination_Area,
                        PutStation = existingEntity?.Put_Station_Nr,
                        Status = existingEntity?.Status,
                        Release = existingEntity?.Release,
                        Processed = existingEntity?.Processed,
                        StatusV10 = result?.DivertStatus,
                        LapCount = result?.LineCount,
                        TrackingId = trackingId,
                        Timestamp = timestamp,
                        CamId = idCam,
                        DivertCode = 1,
                        Info = "Tote could not be diverted"
                    });
                    #endregion
                    return 2;
                }
                #endregion
                #region Commissioner is false
                if (statusCommissioner == false)
                {
                    if (existingEntity.Release >= 2 )
                    {
                        if (existingEntity.Status >= 3 && (existingEntity.WCS_Destination_Area == codeMultiSP || existingEntity.WCS_Destination_Area == codeMultiLP))
                        {
                            #region Scanlog
                            await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                            {
                                ToteLPN = Lpm,
                                VirtualZone = result?.ZoneDivertId,
                                VirtualTote = result?.VirtualTote,
                                Wave = existingEntity?.Wave_Nr,
                                TotesInWave = existingEntity?.Nr_Of_Totes_In_Wave,
                                TotalQty = existingEntity?.Tote_Total_Qty,
                                DestinationArea = existingEntity?.WCS_Destination_Area,
                                PutStation = existingEntity?.Put_Station_Nr,
                                Status = existingEntity?.Status,
                                Release = existingEntity?.Release,
                                Processed = existingEntity?.Processed,
                                StatusV10 = result?.DivertStatus,
                                LapCount = result?.LineCount,
                                TrackingId = trackingId,
                                Timestamp = timestamp,
                                CamId = idCam,
                                DivertCode = 99,
                                Info = "Tote reassigned to the last position"
                            });
                            #endregion
                            return 99;
                        }
                        if (existingEntity.WCS_Destination_Area == codeSingleSM || existingEntity.WCS_Destination_Area == codeSingleSP)
                        {
                            #region Scanlog
                            await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                            {
                                ToteLPN = Lpm,
                                VirtualZone = result?.ZoneDivertId,
                                VirtualTote = result?.VirtualTote,
                                Wave = existingEntity?.Wave_Nr,
                                TotesInWave = existingEntity?.Nr_Of_Totes_In_Wave,
                                TotalQty = existingEntity?.Tote_Total_Qty,
                                DestinationArea = existingEntity?.WCS_Destination_Area,
                                PutStation = existingEntity?.Put_Station_Nr,
                                Status = existingEntity?.Status,
                                Release = existingEntity?.Release,
                                Processed = existingEntity?.Processed,
                                StatusV10 = result?.DivertStatus,
                                LapCount = result?.LineCount,
                                TrackingId = trackingId,
                                Timestamp = timestamp,
                                CamId = idCam,
                                DivertCode = 99,
                                Info = "Tote reassigned to the last position"
                            });
                            #endregion
                            return 99;
                        }

                    }
                    if (idCam == cam10 && existingEntity.Status >= status && (existingEntity.Put_Station_Nr != null && existingEntity.Put_Station_Nr > 0) && existingEntity.Nr_Of_Totes_In_Wave == listWaveComplete?.Count)
                    {
                        if (Lpm.StartsWith("T"))
                        {
                            if (limitsInHighwayPresent.CountMultiTotes >= limitsInHighwayPresent.MultiTotes)
                            {
                                #region Scanlog
                                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                                {
                                    ToteLPN = Lpm,
                                    VirtualZone = result?.ZoneDivertId,
                                    VirtualTote = result?.VirtualTote,
                                    Wave = existingEntity?.Wave_Nr,
                                    TotesInWave = existingEntity?.Nr_Of_Totes_In_Wave,
                                    TotalQty = existingEntity?.Tote_Total_Qty,
                                    DestinationArea = existingEntity?.WCS_Destination_Area,
                                    PutStation = existingEntity?.Put_Station_Nr,
                                    Status = existingEntity?.Status,
                                    Release = existingEntity?.Release,
                                    Processed = existingEntity?.Processed,
                                    StatusV10 = result?.DivertStatus,
                                    LapCount = result?.LineCount,
                                    TrackingId = trackingId,
                                    Timestamp = timestamp,
                                    CamId = idCam,
                                    DivertCode = 1,
                                    Info = "Totes multis at limited capacity"
                                });
                                #endregion
                                return 2;
                            }
                            else
                            {
                                if (existingEntity.WCS_Destination_Area == codeMultiSP || existingEntity.WCS_Destination_Area == codeMultiLP)
                                {
                                    await _e1EXtContext.Database.ExecuteSqlInterpolatedAsync($"UPDATE Tote_Hdr SET Release = 2 FROM Tote_Hdr AS hdr WHERE Tote_LPN = {existingEntity.Tote_LPN} AND Status <> 6 AND Processed IS NULL AND hdr.TIMESTAMP = (SELECT TOP(1) hdr.TIMESTAMP FROM Tote_Hdr WITH (NOLOCK) WHERE Tote_LPN = {existingEntity.Tote_LPN} AND Status <> 6 AND Processed IS NULL ORDER BY TIMESTAMP)");
                                    await _e1EXtContext.SaveChangesAsync();
                                    limitsInHighwayPresent.CountMultiTotes = limitsInHighwayPresent.CountMultiTotes + 1;
                                    await Context.SaveChangesAsync();
                                    #region Scanlog
                                    await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                                    {
                                        ToteLPN = Lpm,
                                        VirtualZone = result?.ZoneDivertId,
                                        VirtualTote = result?.VirtualTote,
                                        Wave = existingEntity?.Wave_Nr,
                                        TotesInWave = existingEntity?.Nr_Of_Totes_In_Wave,
                                        TotalQty = existingEntity?.Tote_Total_Qty,
                                        DestinationArea = existingEntity?.WCS_Destination_Area,
                                        PutStation = existingEntity?.Put_Station_Nr,
                                        Status = existingEntity?.Status,
                                        Release = 2,
                                        Processed = existingEntity?.Processed,
                                        StatusV10 = result?.DivertStatus,
                                        LapCount = result?.LineCount,
                                        TrackingId = trackingId,
                                        Timestamp = timestamp,
                                        CamId = idCam,
                                        DivertCode = 99,
                                        Info = "Tote multi correctly diverted"
                                    });
                                    #endregion
                                    return 99;
                                }
                                #region Scanlog
                                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                                {
                                    ToteLPN = Lpm,
                                    VirtualZone = result?.ZoneDivertId,
                                    VirtualTote = result?.VirtualTote,
                                    Wave = existingEntity?.Wave_Nr,
                                    TotesInWave = existingEntity?.Nr_Of_Totes_In_Wave,
                                    TotalQty = existingEntity?.Tote_Total_Qty,
                                    DestinationArea = existingEntity?.WCS_Destination_Area,
                                    PutStation = existingEntity?.Put_Station_Nr,
                                    Status = existingEntity?.Status,
                                    Release = existingEntity?.Release,
                                    Processed = existingEntity?.Processed,
                                    StatusV10 = result?.DivertStatus,
                                    LapCount = result?.LineCount,
                                    TrackingId = trackingId,
                                    Timestamp = timestamp,
                                    CamId = idCam,
                                    DivertCode = 1,
                                    Info = "Tote could not be diverted"
                                });
                                #endregion
                                return 2;
                            }
                        }
                    }
                    #region Scanlog
                    await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                    {
                        ToteLPN = Lpm,
                        VirtualZone = result?.ZoneDivertId,
                        VirtualTote = result?.VirtualTote,
                        Wave = existingEntity?.Wave_Nr,
                        TotesInWave = existingEntity?.Nr_Of_Totes_In_Wave,
                        TotalQty = existingEntity?.Tote_Total_Qty,
                        DestinationArea = existingEntity?.WCS_Destination_Area,
                        PutStation = existingEntity?.Put_Station_Nr,
                        Status = existingEntity?.Status,
                        Release = existingEntity?.Release,
                        Processed = existingEntity?.Processed,
                        StatusV10 = result?.DivertStatus,
                        LapCount = result?.LineCount,
                        TrackingId = trackingId,
                        Timestamp = timestamp,
                        CamId = idCam,
                        DivertCode = 1,
                        Info = "Tote could not be diverted"
                    });
                    #endregion
                    return 2;
                }
                #endregion
                #endregion
                else
                {
                    #region Scanlog
                    await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                    {
                        ToteLPN = Lpm,
                        VirtualZone = result?.ZoneDivertId,
                        VirtualTote = result?.VirtualTote,
                        Wave = existingEntity?.Wave_Nr,
                        TotesInWave = existingEntity?.Nr_Of_Totes_In_Wave,
                        TotalQty = existingEntity?.Tote_Total_Qty,
                        DestinationArea = existingEntity?.WCS_Destination_Area,
                        PutStation = existingEntity?.Put_Station_Nr,
                        Status = existingEntity?.Status,
                        Release = existingEntity?.Release,
                        Processed = existingEntity?.Processed,
                        StatusV10 = result?.DivertStatus,
                        LapCount = result?.LineCount,
                        TrackingId = trackingId,
                        Timestamp = timestamp,
                        CamId = idCam,
                        DivertCode = 1,
                        Info = "Tote could not be diverted"
                    });
                    #endregion
                    return 2;
                }
            }
            catch (Exception ex)
            {
                #region Scanlog
                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                {
                    ToteLPN = Lpm,
                    TrackingId = trackingId,
                    Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    CamId = idCam,
                    DivertCode = 1,
                    Info = $"Error processing tote {Lpm} in {idCam}"
                });
                #endregion
                throw new Exception($"Error processing request: {ex.Message}");
            }
        }

        public override async Task<ToteInformationE> AddAsync(ToteInformationE entity)
        {
            var toteLpn = entity.ToteLPN;

            var findVTote = await (from ti in Context.ToteInformations
                                   where ti.ToteLPN == toteLpn && ti.DivertStatus == null
                                   select ti).OrderBy(ti => ti.Id).FirstOrDefaultAsync() ?? null;

            if (findVTote == null)
            {
                await Context.AddAsync(entity);
                await Context.SaveChangesAsync();
                return entity;
            }
            else
            {
                throw new Exception($"Unsuccessful register for tote: {entity.VirtualTote} is duplicate");
            }

        }

        public async Task<bool> IsPickingJackpotLine(string camId)
        {
            try
            {
                var camInfo = await Context.Cameras.Where(c => c.CamId == camId).FirstOrDefaultAsync();
                var camDivLineId = await Context.CameraAssignments.Where(ca => ca.CameraId == camInfo.Id).Select(ca => ca.DivertLineId).FirstOrDefaultAsync();
                var jackpotDivId = await Context.PickingJackpotLines.Select(jl => jl.DivertLineId).FirstOrDefaultAsync();

                if (camDivLineId == jackpotDivId)
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error processing request: {ex.Message}");
            }
        }

        public async Task<int> DivertMultiToteCam14(string Lpn, string Cam_Id, int trackingId)
        {
            try
            {
                #region Querys
                #region Search general
                var result = await (from ti in Context.ToteInformations
                                    where ti.ToteLPN == Lpn && ti.DivertStatus == null
                                    select ti)
                                    .FirstOrDefaultAsync() ?? null;
                #endregion
                #region CheckStatusCommissioner
                bool statusCommissioner = await (from c in Context.Commissioners
                                                 where c.Status == true
                                                 select c.Status)
                                                 .FirstOrDefaultAsync();

                var toteHdr = result != null ? await _e1EXtContext.ToteHdrs
                                        .Where(th => th.Tote_LPN == result.ToteLPN && th.Release <= 4 && th.Processed == null).OrderBy(th => th.Timestamp).FirstOrDefaultAsync() ?? null : null;
                #endregion
                #endregion

                if (statusCommissioner == true)
                {
                    if (result != null)
                    {
                        result.DivTimestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        result.TrackingId = trackingId;
                        result.LastCam = Cam_Id;
                        Context.Update(result);
                        await Context.SaveChangesAsync();

                        //var toteHdr = await _e1EXtContext.ToteHdrs
                        //                .Where(th => th.Tote_LPN == result.ToteLPN && th.Release == 3 && th.Processed == null).OrderBy(th => th.Timestamp).FirstOrDefaultAsync() ?? null;
                        if (toteHdr != null)
                        {
                            #region Scanlog
                            await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                            {
                                ToteLPN = Lpn,
                                VirtualZone = result?.ZoneDivertId,
                                VirtualTote = result?.VirtualTote,
                                Wave = toteHdr?.Wave_Nr,
                                TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                                TotalQty = toteHdr?.Tote_Total_Qty,
                                DestinationArea = toteHdr?.WCS_Destination_Area,
                                PutStation = toteHdr?.Put_Station_Nr,
                                Status = toteHdr?.Status,
                                Release = toteHdr?.Release,
                                Processed = toteHdr?.Processed,
                                StatusV10 = result?.DivertStatus,
                                LapCount = result?.LineCount,
                                TrackingId = trackingId,
                                Timestamp = result.DivTimestamp,
                                CamId = Cam_Id,
                                DivertCode = 2,
                                Info = "Tote divert successful"
                            });
                            #endregion
                            return 2;
                        }
                        else
                        {
                            #region Scanlog
                            await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                            {
                                ToteLPN = Lpn,
                                VirtualZone = result?.ZoneDivertId,
                                VirtualTote = result?.VirtualTote,
                                Wave = toteHdr?.Wave_Nr,
                                TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                                TotalQty = toteHdr?.Tote_Total_Qty,
                                DestinationArea = toteHdr?.WCS_Destination_Area,
                                PutStation = toteHdr?.Put_Station_Nr,
                                Status = toteHdr?.Status,
                                Release = toteHdr?.Release,
                                Processed = toteHdr?.Processed,
                                StatusV10 = result?.DivertStatus,
                                LapCount = result?.LineCount,
                                TrackingId = trackingId,
                                Timestamp = result.DivTimestamp,
                                CamId = Cam_Id,
                                DivertCode = 99,
                                Info = "Tote not diverted, does not meet validations for multis"
                            });
                            #endregion
                            return 99;
                        }
                    }
                    else
                    {
                        #region Scanlog
                        await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                        {
                            ToteLPN = Lpn,
                            VirtualZone = result?.ZoneDivertId,
                            VirtualTote = result?.VirtualTote,
                            Wave = toteHdr?.Wave_Nr,
                            TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                            TotalQty = toteHdr?.Tote_Total_Qty,
                            DestinationArea = toteHdr?.WCS_Destination_Area,
                            PutStation = toteHdr?.Put_Station_Nr,
                            Status = toteHdr?.Status,
                            Release = toteHdr?.Release,
                            Processed = toteHdr?.Processed,
                            StatusV10 = result?.DivertStatus,
                            LapCount = result?.LineCount,
                            TrackingId = trackingId,
                            Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                            CamId = Cam_Id,
                            DivertCode = 99,
                            Info = "Tote not registered in V10 system"
                        });
                        #endregion
                        return 99;
                    }
                }
                else
                {
                    #region Scanlog
                    await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                    {
                        ToteLPN = Lpn,
                        VirtualZone = result?.ZoneDivertId,
                        VirtualTote = result?.VirtualTote,
                        Wave = toteHdr?.Wave_Nr,
                        TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                        TotalQty = toteHdr?.Tote_Total_Qty,
                        DestinationArea = toteHdr?.WCS_Destination_Area,
                        PutStation = toteHdr?.Put_Station_Nr,
                        Status = toteHdr?.Status,
                        Release = toteHdr?.Release,
                        Processed = toteHdr?.Processed,
                        StatusV10 = result?.DivertStatus,
                        LapCount = result?.LineCount,
                        TrackingId = trackingId,
                        Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                        CamId = Cam_Id,
                        DivertCode = 99,
                        Info = "Tote not diverted, commissioner off"
                    });
                    #endregion
                    return 99;
                }
            }
            catch (Exception ex)
            {
                #region Scanlog
                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                {
                    ToteLPN = Lpn,
                    TrackingId = trackingId,
                    Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    CamId = Cam_Id,
                    DivertCode = 99,
                    Info = $"Error processing tote {Lpn} in {Cam_Id}"
                });
                #endregion
                throw new Exception($"Error processing request: {ex.Message}");
            }

        }

        public async Task<int> DivertMultiToteCam15(string Lpn, string Cam_Id, int trackingId)
        {
            try
            {
                #region Variables
                var codeLPUT = "LPUT";
                int status = 4; int status2 = 5;
                #endregion
                #region Querys
                #region Search Data
                var result = await (from ti in Context.ToteInformations
                                    where ti.ToteLPN == Lpn && ti.DivertStatus == null
                                    select ti).FirstOrDefaultAsync() ?? null;

                var toteHdr = result != null ? await _e1EXtContext.ToteHdrs
                                    .Where(th => th.Tote_LPN == result.ToteLPN && th.Processed == null)
                                    .OrderBy(th => th.Timestamp).FirstOrDefaultAsync() ?? null : null;
                                
                #endregion
                #endregion
                if (result != null && toteHdr != null && toteHdr.WCS_Destination_Area == codeLPUT)
                {

                    result.DivTimestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    result.TrackingId = trackingId;
                    result.LastCam = Cam_Id;
                    Context.Update(result);
                    await Context.SaveChangesAsync();

                    if (toteHdr != null && (toteHdr.Put_Station_Nr == 1 || toteHdr.Put_Station_Nr == 2) && (toteHdr.Status == 4 || toteHdr.Status == 5))
                    {
                        #region Scanlog
                        await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                        {
                            ToteLPN = Lpn,
                            VirtualZone = result?.ZoneDivertId,
                            VirtualTote = result?.VirtualTote,
                            Wave = toteHdr?.Wave_Nr,
                            TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                            TotalQty = toteHdr?.Tote_Total_Qty,
                            DestinationArea = toteHdr?.WCS_Destination_Area,
                            PutStation = toteHdr?.Put_Station_Nr,
                            Status = toteHdr?.Status,
                            Release = toteHdr?.Release,
                            Processed = toteHdr?.Processed,
                            StatusV10 = result?.DivertStatus,
                            LapCount = result?.LineCount,
                            TrackingId = trackingId,
                            Timestamp = result.DivTimestamp,
                            CamId = Cam_Id,
                            DivertCode = 2,
                            Info = "Tote divert successful"
                        });
                        #endregion
                        return 2;
                    }
                    if (toteHdr != null && toteHdr.Put_Station_Nr == null)
                    {
                        #region Scanlog
                        await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                        {
                            ToteLPN = Lpn,
                            VirtualZone = result?.ZoneDivertId,
                            VirtualTote = result?.VirtualTote,
                            Wave = toteHdr?.Wave_Nr,
                            TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                            TotalQty = toteHdr?.Tote_Total_Qty,
                            DestinationArea = toteHdr?.WCS_Destination_Area,
                            PutStation = toteHdr?.Put_Station_Nr,
                            Status = toteHdr?.Status,
                            Release = toteHdr?.Release,
                            Processed = toteHdr?.Processed,
                            StatusV10 = result?.DivertStatus,
                            LapCount = result?.LineCount,
                            TrackingId = trackingId,
                            Timestamp = result.DivTimestamp,
                            CamId = Cam_Id,
                            DivertCode = 99,
                            Info = "Tote not diverted. Put Station not assigned"
                        });
                        #endregion
                        return 99;
                    }
                    #region Scanlog
                    await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                    {
                        ToteLPN = Lpn,
                        VirtualZone = result?.ZoneDivertId,
                        VirtualTote = result?.VirtualTote,
                        Wave = toteHdr?.Wave_Nr,
                        TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                        TotalQty = toteHdr?.Tote_Total_Qty,
                        DestinationArea = toteHdr?.WCS_Destination_Area,
                        PutStation = toteHdr?.Put_Station_Nr,
                        Status = toteHdr?.Status,
                        Release = toteHdr?.Release,
                        Processed = toteHdr?.Processed,
                        StatusV10 = result?.DivertStatus,
                        LapCount = result?.LineCount,
                        TrackingId = trackingId,
                        Timestamp = result.DivTimestamp,
                        CamId = Cam_Id,
                        DivertCode = 99,
                        Info = "Tote not diverted, does not meet validations"
                    });
                    #endregion
                    return 99;
                }
                else
                {
                    #region Scanlog
                    await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                    {
                        ToteLPN = Lpn,
                        VirtualZone = result?.ZoneDivertId,
                        VirtualTote = result?.VirtualTote,
                        Wave = toteHdr?.Wave_Nr,
                        TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                        TotalQty = toteHdr?.Tote_Total_Qty,
                        DestinationArea = toteHdr?.WCS_Destination_Area,
                        PutStation = toteHdr?.Put_Station_Nr,
                        Status = toteHdr?.Status,
                        Release = toteHdr?.Release,
                        Processed = toteHdr?.Processed,
                        StatusV10 = result?.DivertStatus,
                        LapCount = result?.LineCount,
                        TrackingId = trackingId,
                        Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                        CamId = Cam_Id,
                        DivertCode = 99,
                        Info = "Tote not diverted, WCS_Destiantion_Area not equal to LPUT"
                    });
                    #endregion
                    return 99;
                }
            }
            catch (Exception ex)
            {
                #region Scanlog
                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                {
                    ToteLPN = Lpn,
                    TrackingId = trackingId,
                    Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    CamId = Cam_Id,
                    DivertCode = 99,
                    Info = $"Error processing tote {Lpn} in {Cam_Id}"
                });
                #endregion
                throw new InvalidOperationException($"Error processing request: {ex.Message}");
            }
        }

        public async Task<int> DivertMultiToteCam16(string Lpn, string Cam_Id, int trackingId)
        {
            try
            {
                #region Variables
                var codeSPTA = "SPTA";
                int status = 4; int status2 = 5;
                #endregion
                #region Querys
                #region Search Data
                var result = await (from ti in Context.ToteInformations
                                    where ti.ToteLPN == Lpn && ti.DivertStatus == null
                                    select ti).FirstOrDefaultAsync() ?? null;


                var toteHdr = result != null ? await _e1EXtContext.ToteHdrs
                                    .Where(th => th.Tote_LPN == result.ToteLPN && th.Processed == null)
                                    .OrderBy(th => th.Timestamp).FirstOrDefaultAsync() ?? null : null;
                #endregion
                #endregion
                if (result != null && toteHdr != null && toteHdr.WCS_Destination_Area == codeSPTA)
                {
                    result.DivTimestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    result.TrackingId = trackingId;
                    result.LastCam = Cam_Id;
                    Context.Update(result);
                    await Context.SaveChangesAsync();

                    if (toteHdr != null && toteHdr.Put_Station_Nr == 62 && (toteHdr.Status == 4 || toteHdr.Status == 5))
                    {
                        #region Scanlog
                        await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                        {
                            ToteLPN = Lpn,
                            VirtualZone = result?.ZoneDivertId,
                            VirtualTote = result?.VirtualTote,
                            Wave = toteHdr?.Wave_Nr,
                            TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                            TotalQty = toteHdr?.Tote_Total_Qty,
                            DestinationArea = toteHdr?.WCS_Destination_Area,
                            PutStation = toteHdr?.Put_Station_Nr,
                            Status = toteHdr?.Status,
                            Release = toteHdr?.Release,
                            Processed = toteHdr?.Processed,
                            StatusV10 = result?.DivertStatus,
                            LapCount = result?.LineCount,
                            TrackingId = trackingId,
                            Timestamp = result?.DivTimestamp,
                            CamId = Cam_Id,
                            DivertCode = 2,
                            Info = "Tote divert successful"
                        });
                        #endregion
                        return 2;
                    }
                    if (toteHdr != null && toteHdr.Put_Station_Nr == null)
                    {
                        #region Scanlog
                        await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                        {
                            ToteLPN = Lpn,
                            VirtualZone = result?.ZoneDivertId,
                            VirtualTote = result?.VirtualTote,
                            Wave = toteHdr?.Wave_Nr,
                            TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                            TotalQty = toteHdr?.Tote_Total_Qty,
                            DestinationArea = toteHdr?.WCS_Destination_Area,
                            PutStation = toteHdr?.Put_Station_Nr,
                            Status = toteHdr?.Status,
                            Release = toteHdr?.Release,
                            Processed = toteHdr?.Processed,
                            StatusV10 = result?.DivertStatus,
                            LapCount = result?.LineCount,
                            TrackingId = trackingId,
                            Timestamp = result.DivTimestamp,
                            CamId = Cam_Id,
                            DivertCode = 99,
                            Info = "Tote not diverted. Put Station not assigned"
                        });
                        #endregion
                        return 99;
                    }
                    #region Scanlog
                    await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                    {
                        ToteLPN = Lpn,
                        VirtualZone = result?.ZoneDivertId,
                        VirtualTote = result?.VirtualTote,
                        Wave = toteHdr?.Wave_Nr,
                        TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                        TotalQty = toteHdr?.Tote_Total_Qty,
                        DestinationArea = toteHdr?.WCS_Destination_Area,
                        PutStation = toteHdr?.Put_Station_Nr,
                        Status = toteHdr?.Status,
                        Release = toteHdr?.Release,
                        Processed = toteHdr?.Processed,
                        StatusV10 = result?.DivertStatus,
                        LapCount = result?.LineCount,
                        TrackingId = trackingId,
                        Timestamp = result?.DivTimestamp,
                        CamId = Cam_Id,
                        DivertCode = 99,
                        Info = "Tote not diverted, does not meet validations"
                    });
                    #endregion
                    return 99;
                }
                else
                {
                    #region Scanlog
                    await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                    {
                        ToteLPN = Lpn,
                        VirtualZone = result?.ZoneDivertId,
                        VirtualTote = result?.VirtualTote,
                        Wave = toteHdr?.Wave_Nr,
                        TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                        TotalQty = toteHdr?.Tote_Total_Qty,
                        DestinationArea = toteHdr?.WCS_Destination_Area,
                        PutStation = toteHdr?.Put_Station_Nr,
                        Status = toteHdr?.Status,
                        Release = toteHdr?.Release,
                        Processed = toteHdr?.Processed,
                        StatusV10 = result?.DivertStatus,
                        LapCount = result?.LineCount,
                        TrackingId = trackingId,
                        Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                        CamId = Cam_Id,
                        DivertCode = 99,
                        Info = "Tote not diverted, does not meet validations"
                    });
                    #endregion
                    return 99;
                }
            }
            catch (Exception ex)
            {
                #region Scanlog
                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                {
                    ToteLPN = Lpn,
                    TrackingId = trackingId,
                    Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    CamId = Cam_Id,
                    DivertCode = 99,
                    Info = $"Error processing tote {Lpn} in {Cam_Id}"
                });
                #endregion
                throw new InvalidOperationException($"Error processing request: {ex.Message}");
            }
        }

        public async Task<int> DivertMultiToteCam17(string Lpn, string Cam_Id, int trackingId)
        {
            try
            {
                #region Variables
                var codeSPTA = "SPTA";
                int status = 4; int status2 = 5;
                #endregion
                #region Querys
                #region Search Data
                var result = await (from ti in Context.ToteInformations
                                    where ti.ToteLPN == Lpn && ti.DivertStatus == null
                                    select ti).FirstOrDefaultAsync() ?? null;

                var toteHdr = result != null ? await _e1EXtContext.ToteHdrs
                                    .Where(th => th.Tote_LPN == result.ToteLPN && th.Processed == null)
                                    .OrderBy(th => th.Timestamp).FirstOrDefaultAsync() ?? null : null;
                #endregion
                #endregion
                if (result != null && toteHdr != null && toteHdr.WCS_Destination_Area == codeSPTA)
                {
                    result.DivTimestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    result.TrackingId = trackingId;
                    result.LastCam = Cam_Id;
                    Context.Update(result);
                    await Context.SaveChangesAsync();

                    if (toteHdr != null && toteHdr.Put_Station_Nr == 61 && (toteHdr.Status == 4 || toteHdr.Status == 5))
                    {
                        #region Scanlog
                        await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                        {
                            ToteLPN = Lpn,
                            VirtualZone = result?.ZoneDivertId,
                            VirtualTote = result?.VirtualTote,
                            Wave = toteHdr?.Wave_Nr,
                            TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                            TotalQty = toteHdr?.Tote_Total_Qty,
                            DestinationArea = toteHdr?.WCS_Destination_Area,
                            PutStation = toteHdr?.Put_Station_Nr,
                            Status = toteHdr?.Status,
                            Release = toteHdr?.Release,
                            Processed = toteHdr?.Processed,
                            StatusV10 = result?.DivertStatus,
                            LapCount = result?.LineCount,
                            TrackingId = trackingId,
                            Timestamp = result?.DivTimestamp,
                            CamId = Cam_Id,
                            DivertCode = 2,
                            Info = "Tote divert successful"
                        });
                        #endregion
                        return 2;
                    }
                    if (toteHdr != null && toteHdr.Put_Station_Nr == null)
                    {
                        #region Scanlog
                        await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                        {
                            ToteLPN = Lpn,
                            VirtualZone = result?.ZoneDivertId,
                            VirtualTote = result?.VirtualTote,
                            Wave = toteHdr?.Wave_Nr,
                            TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                            TotalQty = toteHdr?.Tote_Total_Qty,
                            DestinationArea = toteHdr?.WCS_Destination_Area,
                            PutStation = toteHdr?.Put_Station_Nr,
                            Status = toteHdr?.Status,
                            Release = toteHdr?.Release,
                            Processed = toteHdr?.Processed,
                            StatusV10 = result?.DivertStatus,
                            LapCount = result?.LineCount,
                            TrackingId = trackingId,
                            Timestamp = result.DivTimestamp,
                            CamId = Cam_Id,
                            DivertCode = 99,
                            Info = "Tote not diverted. Put Station not assigned"
                        });
                        #endregion
                        return 99;
                    }
                    #region Scanlog
                    await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                    {
                        ToteLPN = Lpn,
                        VirtualZone = result?.ZoneDivertId,
                        VirtualTote = result?.VirtualTote,
                        Wave = toteHdr?.Wave_Nr,
                        TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                        TotalQty = toteHdr?.Tote_Total_Qty,
                        DestinationArea = toteHdr?.WCS_Destination_Area,
                        PutStation = toteHdr?.Put_Station_Nr,
                        Status = toteHdr?.Status,
                        Release = toteHdr?.Release,
                        Processed = toteHdr?.Processed,
                        StatusV10 = result?.DivertStatus,
                        LapCount = result?.LineCount,
                        TrackingId = trackingId,
                        Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                        CamId = Cam_Id,
                        DivertCode = 99,
                        Info = "Tote not diverted, does not meet validations"
                    });
                    #endregion
                    return 99;
                }
                else
                {
                    #region Scanlog
                    await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                    {
                        ToteLPN = Lpn,
                        VirtualZone = result?.ZoneDivertId,
                        VirtualTote = result?.VirtualTote,
                        Wave = toteHdr?.Wave_Nr,
                        TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                        TotalQty = toteHdr?.Tote_Total_Qty,
                        DestinationArea = toteHdr?.WCS_Destination_Area,
                        PutStation = toteHdr?.Put_Station_Nr,
                        Status = toteHdr?.Status,
                        Release = toteHdr?.Release,
                        Processed = toteHdr?.Processed,
                        StatusV10 = result?.DivertStatus,
                        LapCount = result?.LineCount,
                        TrackingId = trackingId,
                        Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                        CamId = Cam_Id,
                        DivertCode = 99,
                        Info = "Tote not diverted, does not meet validations"
                    });
                    #endregion
                    return 99;
                }
            }
            catch (Exception ex)
            {
                #region Scanlog
                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                {
                    ToteLPN = Lpn,
                    TrackingId = trackingId,
                    Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    CamId = Cam_Id,
                    DivertCode = 99,
                    Info = $"Error processing tote {Lpn} in {Cam_Id}"
                });
                #endregion
                throw new InvalidOperationException($"Error processing request: {ex.Message}");
            }
        }

        public async Task<int> DiverTotesInCam11(string Lpn, string Cam_Id, int trackingId)
        {
            try
            {
                #region Queys
                var result = await (from ti in Context.ToteInformations where ti.ToteLPN == Lpn && ti.DivertStatus == null select ti)
                    .FirstOrDefaultAsync() ?? null;

                var toteHdr = result != null ? await _e1EXtContext.ToteHdrs
                                        .Where(th => th.Tote_LPN == result.ToteLPN && th.Status != 6 && th.Release >= 2 && th.Processed == null).OrderBy(th => th.Timestamp).FirstOrDefaultAsync() ?? null : null;
                #endregion
                if (result != null)
                {
                    result.DivTimestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    result.TrackingId = trackingId;
                    result.LastCam = Cam_Id;
                    Context.Update(result);
                    await Context.SaveChangesAsync();


                    if (toteHdr != null)
                    {
                        bool commissionerExist = await Context.Commissioners.AnyAsync(c => c.Status);
                        var limitsInHighwayPresent = await Context.HighwayPikingLanes.FirstOrDefaultAsync();

                        #region Commissioner ON
                        if (commissionerExist == true && (toteHdr.WCS_Destination_Area == "SPTA" || toteHdr.WCS_Destination_Area == "LPUT"))
                        {
                            if (limitsInHighwayPresent.CountMultiTotes <= limitsInHighwayPresent.MultiTotes)
                            {
                                result.LineCount++;
                                Context.Update(result);
                                await Context.SaveChangesAsync();
                                #region Scanlog
                                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                                {
                                    ToteLPN = Lpn,
                                    VirtualZone = result?.ZoneDivertId,
                                    VirtualTote = result?.VirtualTote,
                                    Wave = toteHdr?.Wave_Nr,
                                    TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                                    TotalQty = toteHdr?.Tote_Total_Qty,
                                    DestinationArea = toteHdr?.WCS_Destination_Area,
                                    PutStation = toteHdr?.Put_Station_Nr,
                                    Status = toteHdr?.Status,
                                    Release = toteHdr?.Release,
                                    Processed = toteHdr?.Processed,
                                    StatusV10 = result?.DivertStatus,
                                    LapCount = result?.LineCount,
                                    TrackingId = trackingId,
                                    Timestamp = result?.DivTimestamp,
                                    CamId = Cam_Id,
                                    DivertCode = 2,
                                    Info = "The Tote will remain in transfer 3 and 4."
                                });
                                #endregion
                                return 2;
                            }
                            else
                            {
                                limitsInHighwayPresent.CountMultiTotes--;
                                Context.Update(limitsInHighwayPresent);
                                await Context.SaveChangesAsync();
                                #region Scanlog
                                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                                {
                                    ToteLPN = Lpn,
                                    VirtualZone = result?.ZoneDivertId,
                                    VirtualTote = result?.VirtualTote,
                                    Wave = toteHdr?.Wave_Nr,
                                    TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                                    TotalQty = toteHdr?.Tote_Total_Qty,
                                    DestinationArea = toteHdr?.WCS_Destination_Area,
                                    PutStation = toteHdr?.Put_Station_Nr,
                                    Status = toteHdr?.Status,
                                    Release = toteHdr?.Release,
                                    Processed = toteHdr?.Processed,
                                    StatusV10 = result?.DivertStatus,
                                    LapCount = result?.LineCount,
                                    TrackingId = trackingId,
                                    Timestamp = result?.DivTimestamp,
                                    CamId = Cam_Id,
                                    DivertCode = 99,
                                    Info = "The Tote has been transferred to transfer 1 and 2."
                                });
                                #endregion
                                return 99;
                            }
                        }
                        #endregion
                        #region Commissioner OFF
                        else if (commissionerExist == false && (toteHdr.WCS_Destination_Area == "SPTA" || toteHdr.WCS_Destination_Area == "LPUT"))
                        {
                            #region List Wave
                            var listWaveComplete = await (from list in _e1EXtContext.ToteHdrs
                                                          where list.Nr_Of_Totes_In_Wave == toteHdr.Nr_Of_Totes_In_Wave && list.Wave_Nr == toteHdr.Wave_Nr && list.Status >= 3
                                                          select list).ToListAsync() ?? null;
                            #endregion
                            if (limitsInHighwayPresent.CountMultiTotes <= limitsInHighwayPresent.MultiTotes)
                            {
                                if (toteHdr.Status >= 3 && toteHdr.Nr_Of_Totes_In_Wave == listWaveComplete?.Count)
                                {
                                    result.LineCount++;
                                    Context.Update(result);
                                    await Context.SaveChangesAsync();
                                    #region Scanlog
                                    await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                                    {
                                        ToteLPN = Lpn,
                                        VirtualZone = result?.ZoneDivertId,
                                        VirtualTote = result?.VirtualTote,
                                        Wave = toteHdr?.Wave_Nr,
                                        TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                                        TotalQty = toteHdr?.Tote_Total_Qty,
                                        DestinationArea = toteHdr?.WCS_Destination_Area,
                                        PutStation = toteHdr?.Put_Station_Nr,
                                        Status = toteHdr?.Status,
                                        Release = toteHdr?.Release,
                                        Processed = toteHdr?.Processed,
                                        StatusV10 = result?.DivertStatus,
                                        LapCount = result?.LineCount,
                                        TrackingId = trackingId,
                                        Timestamp = result?.DivTimestamp,
                                        CamId = Cam_Id,
                                        DivertCode = 2,
                                        Info = "The Tote will remain in transfer 3 and 4."
                                    });
                                    #endregion
                                    return 2;
                                }
                                else
                                {
                                    limitsInHighwayPresent.CountMultiTotes--;
                                    Context.Update(limitsInHighwayPresent);
                                    await Context.SaveChangesAsync();
                                    #region Scanlog
                                    await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                                    {
                                        ToteLPN = Lpn,
                                        VirtualZone = result?.ZoneDivertId,
                                        VirtualTote = result?.VirtualTote,
                                        Wave = toteHdr?.Wave_Nr,
                                        TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                                        TotalQty = toteHdr?.Tote_Total_Qty,
                                        DestinationArea = toteHdr?.WCS_Destination_Area,
                                        PutStation = toteHdr?.Put_Station_Nr,
                                        Status = toteHdr?.Status,
                                        Release = toteHdr?.Release,
                                        Processed = toteHdr?.Processed,
                                        StatusV10 = result?.DivertStatus,
                                        LapCount = result?.LineCount,
                                        TrackingId = trackingId,
                                        Timestamp = result?.DivTimestamp,
                                        CamId = Cam_Id,
                                        DivertCode = 99,
                                        Info = "The Tote has been transferred to transfer 1 and 2."
                                    });
                                    #endregion
                                    return 99;
                                }
                            }
                            else
                            {
                                limitsInHighwayPresent.CountMultiTotes--;
                                Context.Update(limitsInHighwayPresent);
                                await Context.SaveChangesAsync();
                                #region Scanlog
                                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                                {
                                    ToteLPN = Lpn,
                                    VirtualZone = result?.ZoneDivertId,
                                    VirtualTote = result?.VirtualTote,
                                    Wave = toteHdr?.Wave_Nr,
                                    TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                                    TotalQty = toteHdr?.Tote_Total_Qty,
                                    DestinationArea = toteHdr?.WCS_Destination_Area,
                                    PutStation = toteHdr?.Put_Station_Nr,
                                    Status = toteHdr?.Status,
                                    Release = toteHdr?.Release,
                                    Processed = toteHdr?.Processed,
                                    StatusV10 = result?.DivertStatus,
                                    LapCount = result?.LineCount,
                                    TrackingId = trackingId,
                                    Timestamp = result?.DivTimestamp,
                                    CamId = Cam_Id,
                                    DivertCode = 99,
                                    Info = "The Tote has been transferred to transfer 1 and 2."
                                });
                                #endregion
                                return 99;
                            }
                        }
                        #endregion
                        #region Singles
                        var limitsCamera10 = await (from l in Context.LimitSettingCameras
                                                    where l.Camera.CamId == "Cam10"
                                                    select l).FirstOrDefaultAsync();
                        if ((toteHdr.WCS_Destination_Area == "SPAC" || toteHdr.WCS_Destination_Area == "SMC3") && toteHdr.Status == 1)
                        {
                            if (limitsCamera10.CounterTote <= limitsCamera10.MaximumCapacity)
                            {
                                result.LineCount++;
                                Context.Update(result);
                                await Context.SaveChangesAsync();
                                #region Scanlog
                                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                                {
                                    ToteLPN = Lpn,
                                    VirtualZone = result?.ZoneDivertId,
                                    VirtualTote = result?.VirtualTote,
                                    Wave = toteHdr?.Wave_Nr,
                                    TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                                    TotalQty = toteHdr?.Tote_Total_Qty,
                                    DestinationArea = toteHdr?.WCS_Destination_Area,
                                    PutStation = toteHdr?.Put_Station_Nr,
                                    Status = toteHdr?.Status,
                                    Release = toteHdr?.Release,
                                    Processed = toteHdr?.Processed,
                                    StatusV10 = result?.DivertStatus,
                                    LapCount = result?.LineCount,
                                    TrackingId = trackingId,
                                    Timestamp = result?.DivTimestamp,
                                    CamId = Cam_Id,
                                    DivertCode = 2,
                                    Info = "The Tote will remain in transfer 3 and 4."
                                });
                                #endregion
                                return 2;
                            }
                            else
                            {
                                limitsCamera10.CounterTote--;
                                Context.Update(limitsCamera10);
                                await Context.SaveChangesAsync();
                                #region Scanlog
                                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                                {
                                    ToteLPN = Lpn,
                                    VirtualZone = result?.ZoneDivertId,
                                    VirtualTote = result?.VirtualTote,
                                    Wave = toteHdr?.Wave_Nr,
                                    TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                                    TotalQty = toteHdr?.Tote_Total_Qty,
                                    DestinationArea = toteHdr?.WCS_Destination_Area,
                                    PutStation = toteHdr?.Put_Station_Nr,
                                    Status = toteHdr?.Status,
                                    Release = toteHdr?.Release,
                                    Processed = toteHdr?.Processed,
                                    StatusV10 = result?.DivertStatus,
                                    LapCount = result?.LineCount,
                                    TrackingId = trackingId,
                                    Timestamp = result?.DivTimestamp,
                                    CamId = Cam_Id,
                                    DivertCode = 99,
                                    Info = "The Tote has been transferred to transfer 1 and 2."
                                });
                                #endregion
                                return 99;
                            }

                        }
                        #endregion

                    }
                    #region Scanlog
                    await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                    {
                        ToteLPN = Lpn,
                        VirtualZone = result?.ZoneDivertId,
                        VirtualTote = result?.VirtualTote,
                        Wave = toteHdr?.Wave_Nr,
                        TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                        TotalQty = toteHdr?.Tote_Total_Qty,
                        DestinationArea = toteHdr?.WCS_Destination_Area,
                        PutStation = toteHdr?.Put_Station_Nr,
                        Status = toteHdr?.Status,
                        Release = toteHdr?.Release,
                        Processed = toteHdr?.Processed,
                        StatusV10 = result?.DivertStatus,
                        LapCount = result?.LineCount,
                        TrackingId = trackingId,
                        Timestamp = result?.DivTimestamp,
                        CamId = Cam_Id,
                        DivertCode = 99,
                        Info = "The Tote has been transferred to transfer 1 and 2."
                    });
                    #endregion
                    return 99;
                }
                #region Scanlog
                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                {
                    ToteLPN = Lpn,
                    VirtualZone = result?.ZoneDivertId,
                    VirtualTote = result?.VirtualTote,
                    Wave = toteHdr?.Wave_Nr,
                    TotesInWave = toteHdr?.Nr_Of_Totes_In_Wave,
                    TotalQty = toteHdr?.Tote_Total_Qty,
                    DestinationArea = toteHdr?.WCS_Destination_Area,
                    PutStation = toteHdr?.Put_Station_Nr,
                    Status = toteHdr?.Status,
                    Release = toteHdr?.Release,
                    Processed = toteHdr?.Processed,
                    StatusV10 = result?.DivertStatus,
                    LapCount = result?.LineCount,
                    TrackingId = trackingId,
                    Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    CamId = Cam_Id,
                    DivertCode = 99,
                    Info = "The Tote has been transferred to transfer 1 and 2."
                });
                #endregion
                return 99;
            }
            catch (Exception ex)
            {
                #region Scanlog
                await _scanlogsRepository.AddScanlog(new ScanlogsAddDto
                {
                    ToteLPN = Lpn,
                    TrackingId = trackingId,
                    Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    CamId = Cam_Id,
                    DivertCode = 99,
                    Info = $"Error processing tote {Lpn} in {Cam_Id}"
                });
                #endregion
                throw new InvalidOperationException($"Error processing request: {ex.Message}");
            }
        }

    }
}