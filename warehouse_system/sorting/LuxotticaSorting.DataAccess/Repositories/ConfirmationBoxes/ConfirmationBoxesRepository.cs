﻿using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.Shared.DTO.ConfirmationBoxes;
using LuxotticaSorting.ApplicationServices.Shared.DTO.RoutingV10S;
using LuxotticaSorting.Core.ConfirmationBoxes;
using LuxotticaSorting.Core.MultiBoxWaves;
using LuxotticaSorting.Core.ScanLogSortings;
using LuxotticaSorting.Core.WCSRoutingV10;
using LuxotticaSorting.DataAccess.Repositories.RoutingV10S;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace LuxotticaSorting.DataAccess.Repositories.ConfirmationBoxes
{
    public class ConfirmationBoxesRepository : Repository<int, ConfirmationBox>
    {
        private readonly SAPContext _SAPcontext;
        private readonly RoutingV10Repository _routingV10Repository;
        private IConfiguration Configuration { get; }

        public ConfirmationBoxesRepository(SortingContext context, SAPContext sAPcontext,RoutingV10Repository routingV10Repository, IConfiguration configuration) : base(context)
        {
            _SAPcontext = sAPcontext;
            _routingV10Repository = routingV10Repository;
            Configuration = configuration;
        }

        public async Task UpdateBoxesQtyBorder(BoxfromRoutingReqDto reqDto)
        {
            try
            {
                #region Updating Qty for Boxes with right conditions
                int? boxesRegisters = await Context.wCSRoutingV10s.Where(r => r.ContainerId == reqDto.ContainerId && r.DivertLane == reqDto.DivertLane && r.Status == "NA").GroupBy(r => new { r.ContainerId, r.DivertLane }).Select(r => r.Count()).FirstOrDefaultAsync();
                int? boxesRegistersExtern = await Context.wCSRoutingV10s.Where(r => r.ContainerId == reqDto.ContainerId && (r.DivertLane == reqDto.DivertLane || r.DivertLane == 30) && (r.Status == "NA" || r.Status == "IN")).GroupBy(r => new { r.ContainerId }).Select(r => r.Count()).FirstOrDefaultAsync();

                if ((boxesRegisters != null && boxesRegisters > 0) && (boxesRegistersExtern != null && boxesRegistersExtern > 0))
                {
                    var listData = new List<WCSRoutingV10> { };
                    if (reqDto.DivertLane == 2 || reqDto.DivertLane == 4)
                    {
                        listData = await Context.wCSRoutingV10s.Where(r => r.ContainerId == reqDto.ContainerId && ((r.DivertLane == 30 && r.Status == "NA") || (r.DivertLane == reqDto.DivertLane && r.Status == "IN"))).Select(r => r).ToListAsync();
                    }

                    var boxToGetInfo = await Context.wCSRoutingV10s.Where(r => r.ContainerId == reqDto.ContainerId && r.DivertLane == reqDto.DivertLane).Select(r => r).FirstOrDefaultAsync();

                    var checkIfExistRecordsCounter = await Context.wCSRoutingV10s.Where(x => x.ContainerId == reqDto.ContainerId && x.BoxId == null && x.BoxType == null
                        && x.CarrierCode == null && x.LogisticAgent == null && x.ConfirmationNumber == null ).FirstOrDefaultAsync();
                    
                    if (checkIfExistRecordsCounter == null)
                    {
                        await AddInTableBorder(boxToGetInfo, reqDto, boxesRegistersExtern, listData);
                        await AddRecordInInternalTable(boxToGetInfo, reqDto, boxesRegisters);
                    }
                    else
                    {
                        //int after2CounterInternalTable = checkIfExistRecordsCounter.Qty;
                        //int? before2CounterInternalTable = after2CounterInternalTable + boxesRegisters;

                        await AddInTableBorder(boxToGetInfo, reqDto, boxesRegisters, listData);
                        checkIfExistRecordsCounter.Qty = (int)boxesRegisters;
                        Context.Update(checkIfExistRecordsCounter);
                        await Context.SaveChangesAsync();
                    }
                 
                }                
                #endregion
            }
            catch (Exception ex)
            {
                throw new Exception($"UpdateBoxesQtyBorder not working: {ex}");
            }
        }

        public async Task AddInTableBorder(WCSRoutingV10 boxToGetInfo, BoxfromRoutingReqDto reqDto, int? boxesRegisters, List<WCSRoutingV10>? dataList)
        {
            try
            {
                var insertQuery = $@"";
                
                if (dataList != null)
                {
                    foreach (var data in dataList)
                    {
                        var insertQueryList = $@"";
                        insertQueryList = $@"
                                INSERT INTO WCS_Routing (
                                BoxId, BoxType, CarrierCode, LogisticAgent, ConfirmationNumber, 
                                ContainerId, ContainerType, Qty, DivertLane, CurrentTs, 
                                Status, SAPSystem
                                )
                                VALUES (
                                '{data.BoxId}', '{data.BoxType}', '{data.CarrierCode}', '{data.LogisticAgent}', '{data.ConfirmationNumber}', 
                                '{data.ContainerId}', 'I', 0, {reqDto.DivertLane}, '{DateTime.Now.ToString("yyyyMMddHHmmssfff")}', 
                                'IN', '{data.SAPSystem}'
                                )";
                        await _SAPcontext.Database.ExecuteSqlRawAsync(insertQueryList);
                        await _SAPcontext.SaveChangesAsync();
                    }
                }

                if (boxToGetInfo.ContainerType == "G")
                {
                    insertQuery = $@"
                                INSERT INTO WCS_Routing (
                                BoxId, BoxType, CarrierCode, LogisticAgent, ConfirmationNumber, 
                                ContainerId, ContainerType, Qty, DivertLane, CurrentTs, 
                                Status, SAPSystem
                                )
                                VALUES (
                                NULL, NULL, NULL, NULL, NULL, 
                                '{reqDto.ContainerId}', 'H', {boxesRegisters}, {boxToGetInfo.DivertLane}, '{DateTime.Now.ToString("yyyyMMddHHmmssfff")}', 
                                'IN', '{boxToGetInfo.SAPSystem}'
                                )";
                }
                else
                {
                    insertQuery = $@"
                                INSERT INTO WCS_Routing (
                                BoxId, BoxType, CarrierCode, LogisticAgent, ConfirmationNumber, 
                                ContainerId, ContainerType, Qty, DivertLane, CurrentTs, 
                                Status, SAPSystem
                                )
                                VALUES (
                                NULL, NULL, NULL, NULL, NULL, 
                                '{reqDto.ContainerId}', 'I', {boxesRegisters}, {boxToGetInfo.DivertLane}, '{DateTime.Now.ToString("yyyyMMddHHmmssfff")}', 
                                'IN', '{boxToGetInfo.SAPSystem}'
                                )";
                }

                await _SAPcontext.Database.ExecuteSqlRawAsync(insertQuery);
                
                await _SAPcontext.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task AddRecordInInternalTable(WCSRoutingV10 boxToGetInfo, BoxfromRoutingReqDto reqDto, int? boxesRegisters)
        {
            try
            {
                var QtyRegister = new WCSRoutingV10
                {
                    BoxId = null,
                    BoxType = null,
                    CarrierCode = null,
                    LogisticAgent = null,
                    ConfirmationNumber = null,
                    ContainerId = reqDto.ContainerId,
                    ContainerType = boxToGetInfo.ContainerType,
                    Qty = (int)boxesRegisters,
                    DivertLane = boxToGetInfo.DivertLane,
                    CurrentTs = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    Status = "NA",
                    SAPSystem = boxToGetInfo.SAPSystem,
                    DivertTs = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    TrackingId = 0,
                    Count = 0
                };

                Context.wCSRoutingV10s.Add(QtyRegister);
                await Context.SaveChangesAsync();
                await _routingV10Repository.AddScanLog(QtyRegister);
            }
            catch (Exception)
            {
                throw;
            }
        }        

        public async Task ReasignWaves(BoxfromRoutingReqDto reqDto)
        {
            try
            {
                //Identificar la linea de cierre respectivas a truck a traves del containerId y buscando si es tipo T
                var container = await Context.Containers.Where(c => c.ContainerId == reqDto.ContainerId).Select(c => c).FirstOrDefaultAsync();
                var conType = await Context.ContainerTypes.Where(ct => ct.Id == container.ContainerTypeId).Select(ct => ct).FirstOrDefaultAsync();
                if (conType.ContainerTypes == "T")
                {
                    //-> Tomar las olas con status null y status in con Count 0 que son de esa linea a travez de su containerId y DivertLane
                    List<MultiBoxWave>? incompleteWaves = await Context.multiBox_Wave.Where(mbw => mbw.ContainerId == container.ContainerId && mbw.DivertLane == reqDto.DivertLane && (mbw.Status == null || (mbw.Status == "IN" && mbw.QtyCount == 0))).Select(mbw => mbw).ToListAsync() ?? null;
                    List<MultiBoxWave>? incompleteWaves4Process = new List<MultiBoxWave>();
                    List<WCSRoutingV10>? boxesInWaves4Process = new List<WCSRoutingV10>();
                    if (incompleteWaves != null)
                    {
                        List<WCSRoutingV10>? boxesInWaves = new List<WCSRoutingV10>();
                        foreach (var incompleteWave in incompleteWaves)
                        {
                            var boxesRecirculated = await Context.wCSRoutingV10s.Where(r =>
                            r.ConfirmationNumber == incompleteWave.ConfirmationNumber && r.ContainerId == reqDto.ContainerId
                            && r.ContainerType == "S" && r.DivertLane == 999 && r.Status == "IN").Select(r => r).ToListAsync() ?? null;

                            var boxesWithDestiny = await Context.wCSRoutingV10s.Where(r =>
                            r.ConfirmationNumber == incompleteWave.ConfirmationNumber && r.ContainerId == reqDto.ContainerId
                            && r.ContainerType == "T" && r.DivertLane == reqDto.DivertLane && r.Status == "IN").Select(r => r).ToListAsync() ?? null;

                            if (boxesRecirculated != null && boxesRecirculated.Count > 0)
                            {
                                boxesInWaves.AddRange(boxesRecirculated);
                            }
                            if (boxesWithDestiny != null && boxesWithDestiny.Count > 0)
                            {
                                boxesInWaves.AddRange(boxesWithDestiny);
                            }

                            if (boxesInWaves.Count > 0)
                            {
                                if(incompleteWave.Status == "IN" && incompleteWave.QtyCount == 0)
                                {
                                    if(incompleteWave.Qty == boxesInWaves.Count)
                                    {
                                        incompleteWaves4Process.Add(incompleteWave);
                                        boxesInWaves4Process.AddRange(boxesInWaves);

                                        boxesInWaves.Clear();
                                    }
                                    else
                                    {
                                        boxesInWaves.Clear();
                                    }
                                }
                                if(incompleteWave.Status == null)
                                {
                                    incompleteWaves4Process.Add(incompleteWave);
                                    boxesInWaves4Process.AddRange(boxesInWaves);

                                    boxesInWaves.Clear();
                                }
                            }
                        }

                        if(incompleteWaves4Process.Count > 0)
                        {
                            if (reqDto.DivertLane == 2)
                            {
                                await ChangeContainers(incompleteWaves4Process, boxesInWaves4Process, 2);
                            }
                            else if (reqDto.DivertLane == 4)
                            {
                                await ChangeContainers(incompleteWaves4Process, boxesInWaves4Process, 4);
                            }
                        }
                    }
                }
                
                //-> Cambiar el containerId de la ola y los registros por uno inventado para esa linea (Container999 para 2, Container888 para 4)


                //Nota para registro de container manual: Cuando sea para truck identificar las olas que estan con containerInventado y cambiarlo por su nuevo containerId manual
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }
        }

        private async Task ChangeContainers(List<MultiBoxWave> waves, List<WCSRoutingV10> boxes, int lane)
        {
            //var tempContainer = lane is 2 ? "CONTAINER2" : "CONTAINER4";

            if(waves != null && waves.Count > 0)
            {
                Context.multiBox_Wave.RemoveRange(waves);
                await Context.SaveChangesAsync();
                //await _routingV10Repository.AddScanLog(QtyRegister);
            }

            if(boxes != null && boxes.Count > 0)
            {
                Context.wCSRoutingV10s.RemoveRange(boxes);
                await Context.SaveChangesAsync();
            }
        }


    }
}
