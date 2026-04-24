﻿using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.Shared.Dto.RoutingV10S;
using LuxotticaSorting.ApplicationServices.Shared.DTO.PrintLabel;
using LuxotticaSorting.ApplicationServices.Shared.DTO.RoutingV10S;
using LuxotticaSorting.Core.BoxTypes;
using LuxotticaSorting.Core.CarrierCodes;
using LuxotticaSorting.Core.ContainerTypes;
using LuxotticaSorting.Core.DivertLanes;
using LuxotticaSorting.Core.LogisticAgents;
using LuxotticaSorting.Core.MappingSorter;
using LuxotticaSorting.Core.MultiBoxWaves;
using LuxotticaSorting.Core.ScanLogSortings;
using LuxotticaSorting.Core.WCSRoutingV10;
using LuxotticaSorting.DataAccess.Repositories.MultiBoxWaves;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.ComponentModel;
using System.Linq;

namespace LuxotticaSorting.DataAccess.Repositories.RoutingV10S
{
    public class RoutingV10Repository : Repository<int, WCSRoutingV10>
    {
        private int divertLaneIdAssignment;
        private readonly SAPContext _sapContext;
        private readonly MultiBoxWaveRepository _multiBoxWaveRepository;
        private IConfiguration Configuration { get; }
        public RoutingV10Repository(SortingContext context, IConfiguration configuration, SAPContext sapContext, MultiBoxWaveRepository multiBoxWaveRepository) : base(context)
        {
            Configuration = configuration;
            _sapContext = sapContext;
            _multiBoxWaveRepository = multiBoxWaveRepository;
        }

        public async Task<WCSRoutingV10?> MetodoParaVerificarDivertLanesDisponibles(WCSRoutingV10 register)
        {
            var boxType = await (from bxt in Context.BoxTypes
                                 where bxt.BoxTypes == register.BoxType
                                 select bxt).FirstOrDefaultAsync();

            var carrierCode = await (from cc in Context.CarrierCodes
                                     where cc.CarrierCodes == register.CarrierCode
                                     select cc.Id).FirstOrDefaultAsync();

            if (boxType == null || carrierCode == null)
            {
                return null;
            }

            var carrierCodeDivertLanelist = await (from cdl in Context.CarrierCodeDivertLaneMappings
                                                   where cdl.CarrierCodeId == carrierCode && cdl.Status == true
                                                   select cdl).ToListAsync();


            var boxtypeDivertLaneStrings = await Context.MappingSorters.Select(dl => dl).ToListAsync();

            List<int> boxtypeDivertLanelist = new List<int>();
            foreach (var registers in boxtypeDivertLaneStrings)
            {

                if (registers.BoxTypeId?.Length > 0 && registers.BoxTypeId != null)
                {
                    var listBoxType = registers.BoxTypeId.Split(',').Select(int.Parse);
                    bool isBoxType = false;
                    foreach (var boxtype in listBoxType)
                    {
                        if (boxtype == boxType.Id)
                        {
                            isBoxType = true;
                        }
                    }

                    if (isBoxType == true)
                    {
                        boxtypeDivertLanelist.Add(registers.DivertLaneId);
                    }
                }
            }

            var boxtypeDivertLanelist2 = carrierCodeDivertLanelist.Select(cdl => cdl.DivertLaneId).Intersect(boxtypeDivertLanelist).ToList();

            if (carrierCodeDivertLanelist.Count == 0 || boxtypeDivertLanelist.Count == 0 || boxtypeDivertLanelist2.Count == 0)
            {
                return register;
            }

            var commonDivertLaneResult = await Context.DivertLanes
                .Where(dl => boxtypeDivertLanelist2.Contains(dl.Id) && dl.Status == true && dl.Full == false)
                .ToListAsync();

            if (commonDivertLaneResult.Count == 0 && (register.ConfirmationNumber == null || register.ConfirmationNumber.Length == 0))
            {
                return register;
            }
            #region MultiBoxing
            else
            if (commonDivertLaneResult.Count == 0 && (register.ConfirmationNumber != null || register.ConfirmationNumber.Length > 0))
            {
                var isWaveRegistered = await _multiBoxWaveRepository.GetMultiBoxWaveAsync(register.ConfirmationNumber);
                //Si la ola existe en el registro de olas checar tambien que la ola esté incompleta 
                if (isWaveRegistered != null && (isWaveRegistered.Status == "IN"))
                {
                    register.DivertLane = 30;
                    register.ContainerType = "P";
                    register.CurrentTs = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    Context.Update(register);
                    await Context.SaveChangesAsync();
                    return register;
                }
            }

            
            if (register.ConfirmationNumber != null && !string.IsNullOrEmpty(register.ConfirmationNumber))
            {
                if (register.Qty > 1 && register.Qty < await MaxCountQtyConfiguration())
                {
                    var divertLaneSelected = commonDivertLaneResult.OrderBy(dl => dl.DivertLanes).Select(dl => dl).FirstOrDefault();//Filtra solo diverts de TRUCK

                    if (divertLaneSelected?.DivertLanes != 2 && divertLaneSelected?.DivertLanes != 4 && divertLaneSelected != null)
                    {
                        #region Section for pull boxes to pallet when the open lines 
                        var waveIsRegistered = await _multiBoxWaveRepository.GetMultiBoxWaveAsync(register.ConfirmationNumber);
                        if (waveIsRegistered != null && (waveIsRegistered.Status == "IN" || waveIsRegistered.Status == null))
                        {
                            //var waveUpdated = await _multiBoxWaveRepository.UpdateWaveCountForWaveNull(register.ConfirmationNumber);

                            register.DivertLane = 30;
                            register.ContainerType = "P";
                            register.CurrentTs = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                            Context.Update(register);
                            await Context.SaveChangesAsync();
                            return register;
                        }
                        else
                        #endregion
                        {
                            return register;
                        }//La ola ya estaba registrada, pero se cerró la linea y la ola estaba completa con status "IN" o NULL ninguna pasó a desviarse entonces como la ola ya estaba registrada para la linea que se cerró entonces deberian de mandarse las cajas a pallet para luego ser confirmadas manualmente. Pero lo que sucede es que hay mas lineas que tienen la misma configuracion que las de truck, por lo que entra en esta sección y devuelven la recirculacion sin confirmar que la ola esté registrada. Entonces, debe agregarse una validacion de que si existe el registro de la ola en la bd con status "in" o NULL entonces las mande a pallet.
                    }
                    int? lane = divertLaneSelected is not null ? divertLaneSelected?.DivertLanes : null;
                    int? containId = divertLaneSelected is not null ? divertLaneSelected.ContainerId : null;

                    var divertLaneAsign = await Context.DivertLanes.Where(dl => lane == dl.DivertLanes).Select(dl => dl).FirstOrDefaultAsync() ?? null;
                    var containerId = await Context.Containers.Where(c => containId == c.Id).Select(c => c).FirstOrDefaultAsync() ?? null;
                    var containerType = new ContainerType();
                    try
                    {
                        containerType = await Context.ContainerTypes.Where(ct => containerId.ContainerTypeId == ct.Id).Select(ct => ct).FirstOrDefaultAsync() ?? null;
                    }
                    catch
                    {
                        containerType = null;
                    }

                    var waveRegistered = await _multiBoxWaveRepository.GetMultiBoxWaveAsync(register.ConfirmationNumber);

                    if ((containerId == null || divertLaneAsign == null || containerType == null) && waveRegistered == null)
                    {
                        //recircular
                        return register;
                    }
                    else
                    if ((containerId == null || divertLaneAsign == null || containerType == null) && (waveRegistered != null && waveRegistered.Status == null))
                    {
                        register.DivertLane = 30;
                        register.ContainerType = "P";
                        register.CurrentTs = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        Context.Update(register);
                        await Context.SaveChangesAsync();
                        return register;
                    }

                    if (await CheckConsistenceQty(register.ConfirmationNumber, register.Qty) == false)
                    {
                        register.DivertLane = 30;
                        register.ContainerType = "P";
                        register.CurrentTs = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        Context.Update(register);
                        await Context.SaveChangesAsync();
                        return register;
                    }


                    var isWaveRegistered = await _multiBoxWaveRepository.GetMultiBoxWaveAsync(register.ConfirmationNumber);//Checar primero si la ola está registrada

                    if (isWaveRegistered == null) //En caso de que sea la primera vez solo se manda a llamar el metodo add para registrarla
                    {
                        var entityAddWave = new MultiBoxWave
                        {
                            ContainerId = containerId.ContainerId,
                            ContainerType = containerType.ContainerTypes,
                            DivertLane = divertLaneAsign.DivertLanes,
                            ConfirmationNumber = register.ConfirmationNumber,
                            Qty = register.Qty
                        };
                        var resultAddWave = await _multiBoxWaveRepository.AddMultiBoxWaveAsync(entityAddWave);

                        if (resultAddWave.Item1 == 1 && resultAddWave.Item2 == true)
                        {
                            register.ContainerId = containerId.ContainerId;
                            Context.wCSRoutingV10s.Update(register);
                            await Context.SaveChangesAsync();
                            return register;
                        }


                    }
                    else//En caso de que lo esté
                    if (isWaveRegistered != null && isWaveRegistered.Status == null)
                    {
                        if (isWaveRegistered.ContainerId != containerId.ContainerId || (isWaveRegistered.QtyCount >= isWaveRegistered.Qty))//Checar el containerId para ver si se debe de desviar la caja a pallet en lugar de agregarla al contador de olas.
                        {
                            register.DivertLane = 30;
                            register.ContainerType = "P";
                            register.CurrentTs = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                            Context.Update(register);
                            await Context.SaveChangesAsync();
                            return register;
                        }//Pallet

                        #region Section for wave count increment 
                        //var boxIdNew = register.BoxId;
                        //int trackingIdNew = (int)register.TrackingId;

                        //Context.wCSRoutingV10s.Remove(register);
                        //await Context.SaveChangesAsync();
                        //return await GetByBoxId(boxIdNew, trackingIdNew);
                        
                        if(string.IsNullOrWhiteSpace(register.ContainerId) && string.IsNullOrEmpty(register.ContainerId))
                        {

                            var entityAddWave = new MultiBoxWave
                            {
                                ContainerId = containerId.ContainerId,
                                ContainerType = containerType.ContainerTypes,
                                DivertLane = divertLaneAsign.DivertLanes,
                                ConfirmationNumber = register.ConfirmationNumber,
                                Qty = register.Qty
                            };
                            var resultAddWave = await _multiBoxWaveRepository.AddMultiBoxWaveAsync(entityAddWave); //Si tiene el mismo containerId se va a registrar con el metodo Add

                            if (resultAddWave.Item1 == 0 && resultAddWave.Item2 == true)
                            {
                                register.ContainerId = containerId.ContainerId;
                                Context.wCSRoutingV10s.Update(register);
                                await Context.SaveChangesAsync();

                                var isWaveCompleted = await _multiBoxWaveRepository.GetMultiBoxWaveAsync(register.ConfirmationNumber);

                                if (isWaveCompleted.Status == "IN" && isWaveCompleted.QtyCount == 0)//Si el resultado del metodo getMultiboxWave dice que se ha completado la ola, se debe de asignar el destino de manera correcta para todos los registros de la ola.
                                {

                                    var boxesInWave = await Context.wCSRoutingV10s.Where(r => r.ConfirmationNumber == register.ConfirmationNumber && r.ContainerType == "S").Select(r => r).ToListAsync();

                                    foreach (var box in boxesInWave)
                                    {
                                        box.ContainerId = containerId.ContainerId;
                                        box.ContainerType = containerType.ContainerTypes;
                                        box.DivertLane = divertLaneAsign.DivertLanes;
                                        box.CurrentTs = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                                        box.Count = 0;

                                    }

                                    Context.wCSRoutingV10s.UpdateRange(boxesInWave);


                                    register.ContainerId = containerId.ContainerId;
                                    register.ContainerType = containerType.ContainerTypes;
                                    register.DivertLane = divertLaneAsign.DivertLanes;
                                    register.Count = 0;

                                    Context.Update(register);
                                    await Context.SaveChangesAsync();

                                    return register;
                                }
                                else//Si el resultado del metodo Add dice que no se ha completado la ola entonces se manda la caja al metodo de recircular multiboxings.
                                {
                                    return register;
                                }
                            }
                        }
                        
                        #endregion

                    }
                    else
                    if (isWaveRegistered != null && (isWaveRegistered.Status == "IN" || isWaveRegistered.Status == "NA"))
                    {
                        register.DivertLane = 30;
                        register.ContainerType = "P";
                        register.CurrentTs = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        Context.Update(register);
                        await Context.SaveChangesAsync();
                        return register;
                    }
                }
                else
                if (register.Qty >= await MaxCountQtyConfiguration())
                {
                    register.DivertLane = 30;
                    register.ContainerType = "P";
                    register.CurrentTs = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    Context.Update(register);
                    await Context.SaveChangesAsync();
                    return register;
                }
            }
            #endregion
            else
            {
                var containersId = await Context.Containers
                .Where(csI => commonDivertLaneResult.Select(dl => dl.ContainerId).Contains(csI.Id))
                .Select(csI => csI.ContainerId)
                .ToListAsync();

                if (containersId.Count == 0)
                {
                    return register;
                }

                #region 1:1
                var existCountBoxTypeInRouting = await (
                    from bx in Context.wCSRoutingV10s
                    where bx.BoxType == register.BoxType && bx.LogisticAgent == register.LogisticAgent
                          && containersId.Contains(bx.ContainerId)
                    select bx
                ).ToListAsync();

                for (int i = 0; i <= existCountBoxTypeInRouting.Count; i++)
                {
                    var divertLaneId = NextDivertLaneId(commonDivertLaneResult, i);
                    divertLaneIdAssignment = divertLaneId;
                }
                #endregion

                var divertLane = await (from dl in Context.DivertLanes
                                        where dl.Id == divertLaneIdAssignment
                                        select dl).FirstOrDefaultAsync() ?? null;

                if (divertLane == null)
                {
                    return register;
                }

                var containerId = await (from ct in Context.Containers
                                         where ct.Id == divertLane.ContainerId
                                         select ct).FirstOrDefaultAsync() ?? null;

                ContainerType? containerType = null;

                if (containerId != null)
                {
                    containerType = await (from ct in Context.ContainerTypes
                                           where ct.Id == containerId.ContainerTypeId
                                           select ct).FirstOrDefaultAsync() ?? null;
                }
                if (containerId == null && (divertLane.DivertLanes >= 1 && divertLane.DivertLanes <= 4))
                {
                    return register;
                }
                if (containerId == null || containerType == null)
                {
                    return register;
                }


                register.ContainerId = containerId.ContainerId;
                register.ContainerType = containerType.ContainerTypes;
                register.DivertLane = divertLane.DivertLanes;
                register.Count = 0;

                Context.Update(register);
                await Context.SaveChangesAsync();

                return register;
            }
            return register;
        }

        private async Task<int?> MaxCountQtyConfiguration()
        {
            string query = $"SELECT MaxBoxCount FROM ConfigurationsSetting";
            using (SqlConnection connection = new SqlConnection(Configuration.GetConnectionString("Shipping_connection")))
            {
                await connection.OpenAsync();
                SqlCommand command = new SqlCommand(query, connection);
                var reader = await command.ExecuteReaderAsync();
                if (reader.Read())
                {
                    int result = (int)reader["MaxBoxCount"];
                    await connection.CloseAsync();
                    return result;
                }
                await connection.CloseAsync();
                return null;
            }
        }

        private async Task<bool> CheckConsistenceQty(string confirmationNumber, int qty)
        {
            string query = $"SELECT COUNT(ConfirmationNumber) AS Count from SAP_Orders where ConfirmationNumber = '{confirmationNumber}'"; //<--- POSIBLE AGREGAR STATUS = IN PARA DIFERENCIAR REGISTROS A NO TOMAR EN CUENTA
            using (SqlConnection connection = new SqlConnection(Configuration.GetConnectionString("SAP_connection")))
            {
                await connection.OpenAsync();
                SqlCommand command = new SqlCommand(query, connection);
                var reader = await command.ExecuteReaderAsync();

                if (reader.Read())
                {
                    var countBoxes = (int)reader["Count"];
                    if (countBoxes == qty)
                    {
                        await connection.CloseAsync();
                        return true;
                    }
                    await connection.CloseAsync();
                    return false;
                }
                await connection.CloseAsync();
            }

            return false;
        }

        private async Task<bool> CheckContainTruckLane(List<DivertLane> lanelist)
        {
            try
            {
                bool contains = false;
                foreach(var lane in lanelist)
                {
                    if(lane.DivertLanes == 2 || lane.DivertLanes == 4)
                    {
                        contains = true;
                    }
                }
                return contains;
            }catch(Exception ex)
            {
                return false;
            }
        }

        public async Task<WCSRoutingV10> GetByBoxId(string boxId, int trackingId)
        {
            var existInTheDB = await (from bx in Context.wCSRoutingV10s
                                      where bx.BoxId == boxId && bx.Status == "IN"
                                      select bx).FirstOrDefaultAsync() ?? null;

            if (existInTheDB != null)
            {
                existInTheDB.Count++;
                existInTheDB.TrackingId = trackingId;
                Context.Update(existInTheDB);
                await Context.SaveChangesAsync();
                //await AddScanLog(existInTheDB);

                if (existInTheDB.DivertLane == 999)
                {
                    //Metodo que checa a partir de la informacion proporcionada por la caja si es posible que ahora si haya divertlines disponibles para desviar
                    var result = await MetodoParaVerificarDivertLanesDisponibles(existInTheDB);
                    return result;
                }
                await AddScanLog(existInTheDB);

                return existInTheDB;
            }

            string query = $"SELECT * FROM SAP_Orders WHERE BoxId = '{boxId}' AND Status = 'IN'"; //<--- POSIBLE AGREGAR STATUS = IN PARA DIFERENCIAR REGISTROS A NO TOMAR EN CUENTA
            using (SqlConnection connection = new SqlConnection(Configuration.GetConnectionString("SAP_connection")))
            {
                await connection.OpenAsync();
                SqlCommand command = new SqlCommand(query, connection);
                var reader = await command.ExecuteReaderAsync();

                if (reader.Read())
                {
                    #region result data variables
                    string? boxTypeSAP = reader[$"{OrderDataModelDto.BoxType}"] is DBNull ? null : (string)reader[$"{OrderDataModelDto.BoxType}"];
                    string? carrierCodeSAP = reader[$"{OrderDataModelDto.CarrierCode}"] is DBNull ? "" : (string)reader[$"{OrderDataModelDto.CarrierCode}"];
                    string? logisticAgentSAP = reader[$"{OrderDataModelDto.LogisticAgent}"] is DBNull ? "" : (string)reader[$"{OrderDataModelDto.LogisticAgent}"];
                    string? confirmationNumberSAP = reader[$"{OrderDataModelDto.ConfirmationNumber}"] is DBNull ? "" : (string)reader[$"{OrderDataModelDto.ConfirmationNumber}"];
                    decimal? qtyValueStringSAP = reader["Qty"] is DBNull ? null : (Decimal)reader["Qty"];
                    string? statusSAP = reader[$"{OrderDataModelDto.Status}"] is DBNull ? "" : (string)reader[$"{OrderDataModelDto.Status}"];
                    string? sapSystemSAP = reader[$"{OrderDataModelDto.SAPSystem}"] is DBNull ? "" : (string)reader[$"{OrderDataModelDto.SAPSystem}"];
                    string? currentTsSystem = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    int? qtySAP = Convert.ToInt32(qtyValueStringSAP);
                    #endregion
                    #region Querys

                    if (boxTypeSAP == "" || carrierCodeSAP == "")
                    {
                        var result = await AddBoxDtoValuePalletAsync(boxId, boxTypeSAP, carrierCodeSAP, logisticAgentSAP, confirmationNumberSAP, currentTsSystem, sapSystemSAP);
                        return result;
                    }

                    var boxType = await (from bxt in Context.BoxTypes
                                         where bxt.BoxTypes == boxTypeSAP
                                         select bxt).FirstOrDefaultAsync();

                    if (boxType == null)
                    {
                        var result = await AddBoxDtoValuePalletAsync(boxId, boxTypeSAP, carrierCodeSAP, logisticAgentSAP, confirmationNumberSAP, currentTsSystem, sapSystemSAP);
                        return result;
                    }

                    var carrierCode = await (from cc in Context.CarrierCodes
                                             where cc.CarrierCodes == carrierCodeSAP
                                             select cc.Id).FirstOrDefaultAsync();

                    var carrierCodeDivertLanelist = await (from cdl in Context.CarrierCodeDivertLaneMappings
                                                           where cdl.CarrierCodeId == carrierCode && cdl.Status == true
                                                           select cdl).ToListAsync();



                    //-----------------------------------------

                    var boxtypeDivertLaneStrings = await Context.MappingSorters.Select(dl => dl).ToListAsync();

                    //convertir BoxType a array
                    List<int> boxtypeDivertLanelist = new List<int>();
                    foreach (var registers in boxtypeDivertLaneStrings)
                    {

                        if (registers.BoxTypeId?.Length > 0 && registers.BoxTypeId != null)
                        {
                            var listBoxType = registers.BoxTypeId.Split(',').Select(int.Parse);
                            bool isBoxType = false;
                            foreach (var boxtype in listBoxType)
                            {
                                if (boxtype == boxType.Id)
                                {
                                    isBoxType = true;
                                }
                            }

                            if (isBoxType == true)
                            {
                                boxtypeDivertLanelist.Add(registers.DivertLaneId);
                            }
                        }
                    }

                    //-----------------------------------------
                    var boxtypeDivertLanelist2 = carrierCodeDivertLanelist.Select(cdl => cdl.DivertLaneId).Intersect(boxtypeDivertLanelist).ToList();

                    if (carrierCodeDivertLanelist.Count == 0 || boxtypeDivertLanelist.Count == 0 || boxtypeDivertLanelist2.Count == 0)
                    {
                        var result = await AddBoxDtoValuePalletAsync(boxId, boxTypeSAP, carrierCodeSAP, logisticAgentSAP, confirmationNumberSAP, currentTsSystem, sapSystemSAP);
                        return result;
                    }

                    var commonDivertLaneResult = await Context.DivertLanes
                        .Where(dl => boxtypeDivertLanelist2.Contains(dl.Id) && dl.Status == true && dl.Full == false)
                        .ToListAsync();

                    var lanesWithoutfilter = await Context.DivertLanes.Where(dl => boxtypeDivertLanelist2.Contains(dl.Id)).ToListAsync();

                    if (commonDivertLaneResult.Count == 0 && (string.IsNullOrEmpty(confirmationNumberSAP)))
                    {
                        var result = await MetodoParaRegistrarCajaSinDestino(boxId, boxTypeSAP, carrierCodeSAP, logisticAgentSAP, confirmationNumberSAP, currentTsSystem, sapSystemSAP);//Crear nuevo metodo para registrar la caja con divert 999 
                        return result;
                    }
                    #region Multis
                    else
                    if (commonDivertLaneResult.Count == 0 && !string.IsNullOrEmpty(confirmationNumberSAP))
                    {
                        var isWaveRegistered = await _multiBoxWaveRepository.GetMultiBoxWaveAsync(confirmationNumberSAP);
                        //Si la ola existe en el registro de olas checar tambien que la ola esté incompleta 
                        if(isWaveRegistered != null && (isWaveRegistered.Status == "IN"))
                        {
                            var result = await AddBoxDtoValuePalletAsync(boxId, boxTypeSAP, carrierCodeSAP, logisticAgentSAP, confirmationNumberSAP, currentTsSystem, sapSystemSAP);
                            return result;
                        }else
                        if (isWaveRegistered != null && (isWaveRegistered.Status == null))
                        {

                            if(await CheckContainTruckLane(lanesWithoutfilter) == false)
                            {
                                var result = await AddBoxDtoValuePalletAsync(boxId, boxTypeSAP, carrierCodeSAP, logisticAgentSAP, confirmationNumberSAP, currentTsSystem, sapSystemSAP);
                                return result;
                            }
                            else
                            {
                                #region Section for update WaveCount and close Wave for Status Null

                                var waveUpdated = await _multiBoxWaveRepository.UpdateWaveCountForWaveNull(confirmationNumberSAP);

                                if (waveUpdated)
                                {
                                    var result = await AddBoxDtoValuePalletAsync(boxId, boxTypeSAP, carrierCodeSAP, logisticAgentSAP, confirmationNumberSAP, currentTsSystem, sapSystemSAP);
                                    return result;
                                }
                                else
                                {
                                    var result = await MetodoParaRegistrarCajaMultiboxSinDestino(boxId, boxTypeSAP, carrierCodeSAP, logisticAgentSAP, confirmationNumberSAP, currentTsSystem, sapSystemSAP, "", (int)qtySAP);//Crear nuevo metodo para registrar la caja con divert 999 
                                    return result;
                                }

                                #endregion
                            }
                        }
                    }


                    
                    if (!string.IsNullOrEmpty(confirmationNumberSAP))
                    {
                        if (qtySAP > 1 && qtySAP < await MaxCountQtyConfiguration())
                        {
                            //nota filtrar solo diverts de TRUCK

                            var divertLaneSelected = commonDivertLaneResult.OrderBy(dl => dl.DivertLanes).Select(dl => dl).FirstOrDefault() ?? null;

                            if (divertLaneSelected?.DivertLanes != 2 && divertLaneSelected?.DivertLanes != 4 && divertLaneSelected != null)
                            {

                                #region Section for pull boxes to pallet when the open lines 

                                var waveIsRegistered = await _multiBoxWaveRepository.GetMultiBoxWaveAsync(confirmationNumberSAP);
                                if(waveIsRegistered != null && (waveIsRegistered.Status == "IN" || waveIsRegistered.Status == null))
                                {
                                    WCSRoutingV10 result = new WCSRoutingV10();
                                    if(waveIsRegistered.Status == null)
                                    {
                                        result = await AddBoxDtoValuePalletAsync(boxId, boxTypeSAP, carrierCodeSAP, logisticAgentSAP, "", currentTsSystem, sapSystemSAP);
                                    }
                                    else
                                    {
                                        result = await AddBoxDtoValuePalletForMultiAsync(boxId, boxTypeSAP, carrierCodeSAP, logisticAgentSAP, confirmationNumberSAP, waveIsRegistered.ContainerId, currentTsSystem, sapSystemSAP);
                                        
                                    }
                                    //var waveUpdated = await _multiBoxWaveRepository.UpdateWaveCountForWaveNull(confirmationNumberSAP);
                                    return result;
                                }
                                else

                                #endregion
                                {
                                    var result = await MetodoParaRegistrarCajaMultiboxSinDestino(boxId, boxTypeSAP, carrierCodeSAP, logisticAgentSAP, confirmationNumberSAP, currentTsSystem, sapSystemSAP, "", (int)qtySAP);//Crear nuevo metodo para registrar la caja con divert 999 
                                    return result;
                                }
                                //La ola ya estaba registrada, pero se cerró la linea y la ola estaba completa con status "IN" o NULL ninguna pasó a desviarse entonces como la ola ya estaba registrada para la linea que se cerró entonces deberian de mandarse las cajas a pallet para luego ser confirmadas manualmente. Pero lo que sucede es que hay mas lineas que tienen la misma configuracion que las de truck, por lo que entra en esta sección y devuelven la recirculacion sin confirmar que la ola esté registrada. Entonces, debe agregarse una validacion de que si existe el registro de la ola en la bd con status "in" o NULL entonces las mande a pallet.
                            }

                            int? lane = divertLaneSelected is not null ? divertLaneSelected?.DivertLanes : null;
                            int? containId = divertLaneSelected is not null ? divertLaneSelected.ContainerId : null;

                            var divertLaneAsign = await Context.DivertLanes.Where(dl => lane == dl.DivertLanes).Select(dl => dl).FirstOrDefaultAsync() ?? null;
                            var containerId = await Context.Containers.Where(c => containId == c.Id).Select(c => c).FirstOrDefaultAsync() ?? null;
                            var containerType = new ContainerType();
                            try
                            {
                                containerType = await Context.ContainerTypes.Where(ct => containerId.ContainerTypeId == ct.Id).Select(ct => ct).FirstOrDefaultAsync() ?? null;
                            }
                            catch
                            {
                                containerType = null;
                            }

                            var waveRegistered = await _multiBoxWaveRepository.GetMultiBoxWaveAsync(confirmationNumberSAP);

                            if ((containerId == null || divertLaneAsign == null || containerType == null) && waveRegistered == null)
                            {
                                //recircular
                                var result = await MetodoParaRegistrarCajaMultiboxSinDestino(boxId, boxTypeSAP, carrierCodeSAP, logisticAgentSAP, confirmationNumberSAP, currentTsSystem, sapSystemSAP, containerId?.ContainerId, (int)qtySAP);//Crear nuevo metodo para registrar la caja con divert 999 
                                return result;
                            }else
                            if((containerId == null || divertLaneAsign == null || containerType == null) && (waveRegistered != null && waveRegistered.Status == null))
                            {
                                var result = await AddBoxDtoValuePalletAsync(boxId, boxTypeSAP, carrierCodeSAP, logisticAgentSAP, confirmationNumberSAP, currentTsSystem, sapSystemSAP);
                                return result;
                            }

                            if (await CheckConsistenceQty(confirmationNumberSAP, (int)qtySAP) == false)
                            {
                                var result = await AddBoxDtoValuePalletAsync(boxId, boxTypeSAP, carrierCodeSAP, logisticAgentSAP, confirmationNumberSAP, currentTsSystem, sapSystemSAP);
                                return result;
                            }

                            var isWaveRegistered = await _multiBoxWaveRepository.GetMultiBoxWaveAsync(confirmationNumberSAP);//Checar primero si la ola está registrada

                            if (isWaveRegistered == null) //En caso de que sea la primera vez solo se manda a llamar el metodo add para registrarla
                            {
                                var entityAddWave = new MultiBoxWave
                                {
                                    ContainerId = containerId.ContainerId,
                                    ContainerType = containerType.ContainerTypes,
                                    DivertLane = divertLaneAsign.DivertLanes,
                                    ConfirmationNumber = confirmationNumberSAP,
                                    Qty = (int)qtySAP
                                };
                                var resultAddWave = await _multiBoxWaveRepository.AddMultiBoxWaveAsync(entityAddWave);

                                if (resultAddWave.Item1 == 1 && resultAddWave.Item2 == true)
                                {
                                    var addRecircle = await MetodoParaRegistrarCajaMultiboxSinDestino(boxId, boxTypeSAP, carrierCodeSAP, logisticAgentSAP, confirmationNumberSAP, currentTsSystem, sapSystemSAP, containerId.ContainerId, (int)qtySAP);
                                    return addRecircle;
                                }


                            }
                            else//En caso de que lo esté
                            if (isWaveRegistered != null && isWaveRegistered.Status == null)
                            {
                                if (isWaveRegistered.ContainerId != containerId.ContainerId || (isWaveRegistered.QtyCount >= isWaveRegistered.Qty))//Checar el containerId para ver si se debe de desviar la caja a pallet en lugar de agregarla al contador de olas.
                                {
                                    var waveUpdated = await _multiBoxWaveRepository.UpdateWaveCountForWaveNull(confirmationNumberSAP);
                                    if (waveUpdated)
                                    {
                                        var result = await AddBoxDtoValuePalletAsync(boxId, boxTypeSAP, carrierCodeSAP, logisticAgentSAP, confirmationNumberSAP, currentTsSystem, sapSystemSAP);
                                        return result;
                                    }
                                    else
                                    {
                                        var result = await MetodoParaRegistrarCajaMultiboxSinDestino(boxId, boxTypeSAP, carrierCodeSAP, logisticAgentSAP, confirmationNumberSAP, currentTsSystem, sapSystemSAP, "", (int)qtySAP);//Crear nuevo metodo para registrar la caja con divert 999 
                                        return result;
                                    }
                                }

                                var entityAddWave = new MultiBoxWave
                                {
                                    ContainerId = containerId.ContainerId,
                                    ContainerType = containerType.ContainerTypes,
                                    DivertLane = divertLaneAsign.DivertLanes,
                                    ConfirmationNumber = confirmationNumberSAP,
                                    Qty = (int)qtySAP
                                };
                                var resultAddWave = await _multiBoxWaveRepository.AddMultiBoxWaveAsync(entityAddWave); //Si tiene el mismo containerId se va a registrar con el metodo Add

                                if (resultAddWave.Item1 == 0 && resultAddWave.Item2 == true)
                                {
                                    var isWaveCompleted = await _multiBoxWaveRepository.GetMultiBoxWaveAsync(confirmationNumberSAP);

                                    if (isWaveCompleted.Status == "IN" && isWaveCompleted.QtyCount == 0)//Si el resultado del metodo getMultiboxWave dice que se ha completado la ola, se debe de asignar el destino de manera correcta para todos los registros de la ola.
                                    {

                                        var boxesInWave = await Context.wCSRoutingV10s.Where(r => r.ConfirmationNumber == confirmationNumberSAP && r.ContainerId == containerId.ContainerId && r.ContainerType == "S").Select(r => r).ToListAsync();

                                        foreach (var box in boxesInWave)
                                        {
                                            box.ContainerId = containerId.ContainerId;
                                            box.ContainerType = containerType.ContainerTypes;
                                            box.DivertLane = divertLaneAsign.DivertLanes;
                                            box.CurrentTs = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                                            box.Count = 0;

                                        }

                                        Context.wCSRoutingV10s.UpdateRange(boxesInWave);

                                        WCSRoutingV10 boxAddDto = new WCSRoutingV10
                                        {
                                            BoxId = boxId,
                                            BoxType = boxTypeSAP,
                                            CarrierCode = carrierCodeSAP,
                                            LogisticAgent = logisticAgentSAP,
                                            ConfirmationNumber = confirmationNumberSAP,
                                            ContainerId = containerId.ContainerId,
                                            ContainerType = containerType.ContainerTypes,
                                            Qty = (int)qtySAP,
                                            DivertLane = divertLaneAsign.DivertLanes,
                                            CurrentTs = currentTsSystem,
                                            Status = "IN",
                                            SAPSystem = sapSystemSAP,
                                            Count = 0
                                        };


                                        await Context.AddAsync(boxAddDto);
                                        await AddScanLog(boxAddDto);
                                        await Context.SaveChangesAsync();

                                    }
                                    else//Si el resultado del metodo Add dice que no se ha completado la ola entonces se manda la caja al metodo de recircular multiboxings.
                                    {
                                        var addRecircle = await MetodoParaRegistrarCajaMultiboxSinDestino(boxId, boxTypeSAP, carrierCodeSAP, logisticAgentSAP, confirmationNumberSAP, currentTsSystem, sapSystemSAP, containerId.ContainerId, (int)qtySAP);
                                        return addRecircle;
                                    }
                                }

                            }
                            else
                            if(isWaveRegistered != null && (isWaveRegistered.Status == "IN" || isWaveRegistered.Status == "NA"))
                            {
                                var result = new WCSRoutingV10();
                                if(isWaveRegistered.Status == "IN")
                                {
                                    result = await AddBoxDtoValuePalletForMultiAsync(boxId, boxTypeSAP, carrierCodeSAP, logisticAgentSAP, confirmationNumberSAP, isWaveRegistered.ContainerId, currentTsSystem, sapSystemSAP);
                                }
                                else
                                {
                                    result = await AddBoxDtoValuePalletAsync(boxId, boxTypeSAP, carrierCodeSAP, logisticAgentSAP, confirmationNumberSAP, currentTsSystem, sapSystemSAP);
                                }
                                return result;
                            }

                        }
                        else
                        if (qtySAP >= await MaxCountQtyConfiguration())
                        {
                            //mandar a pallet
                            var result = await AddBoxDtoValuePalletAsync(boxId, boxTypeSAP, carrierCodeSAP, logisticAgentSAP, confirmationNumberSAP, currentTsSystem, sapSystemSAP);
                            return result;
                        }
                    }
                    #endregion
                    else
                    {
                        var containersId = await Context.Containers
                        .Where(csI => commonDivertLaneResult.Select(dl => dl.ContainerId).Contains(csI.Id))
                        .Select(csI => csI.ContainerId)
                        .ToListAsync();

                        if (containersId.Count == 0)
                        {
                            var result = await MetodoParaRegistrarCajaSinDestino(boxId, boxTypeSAP, carrierCodeSAP, logisticAgentSAP, confirmationNumberSAP, currentTsSystem, sapSystemSAP);//Crear nuevo metodo para registrar la caja con divert 999 
                            return result;
                        }

                        #region 1:1
                        var existCountBoxTypeInRouting = await (
                            from bx in Context.wCSRoutingV10s
                            where bx.BoxType == boxTypeSAP && bx.LogisticAgent == logisticAgentSAP
                                  && containersId.Contains(bx.ContainerId)
                            select bx
                        ).ToListAsync();

                        for (int i = 0; i <= existCountBoxTypeInRouting.Count; i++)
                        {
                            var divertLaneId = NextDivertLaneId(commonDivertLaneResult, i);
                            divertLaneIdAssignment = divertLaneId;
                        }
                        #endregion

                        var divertLane = await (from dl in Context.DivertLanes
                                                where dl.Id == divertLaneIdAssignment
                                                select dl).FirstOrDefaultAsync() ?? null;

                        if (divertLane == null)
                        {
                            var result = await AddBoxDtoValuePalletAsync(boxId, boxTypeSAP, carrierCodeSAP, logisticAgentSAP, confirmationNumberSAP, currentTsSystem, sapSystemSAP);
                            return result;
                        }

                        var containerId = await (from ct in Context.Containers
                                                 where ct.Id == divertLane.ContainerId
                                                 select ct).FirstOrDefaultAsync() ?? null;

                        ContainerType? containerType = null;

                        if (containerId != null)
                        {
                            containerType = await (from ct in Context.ContainerTypes
                                                   where ct.Id == containerId.ContainerTypeId
                                                   select ct).FirstOrDefaultAsync() ?? null;
                        }
                        if (containerId == null && (divertLane.DivertLanes >= 1 && divertLane.DivertLanes <= 4))
                        {
                            var result = await MetodoParaRegistrarCajaSinDestino(boxId, boxTypeSAP, carrierCodeSAP, logisticAgentSAP, confirmationNumberSAP, currentTsSystem, sapSystemSAP); //Crear nuevo metodo
                            return result;
                        }
                        if (containerId == null || containerType == null)
                        {
                            var result = await MetodoParaRegistrarCajaSinDestino(boxId, boxTypeSAP, carrierCodeSAP, logisticAgentSAP, confirmationNumberSAP, currentTsSystem, sapSystemSAP);
                            return result;
                        }


                        #endregion

                        WCSRoutingV10 boxAddDto = new WCSRoutingV10
                        {
                            BoxId = boxId,
                            BoxType = boxTypeSAP,
                            CarrierCode = carrierCodeSAP,
                            LogisticAgent = logisticAgentSAP,
                            ConfirmationNumber = confirmationNumberSAP,
                            ContainerId = containerId.ContainerId,
                            ContainerType = containerType.ContainerTypes,
                            Qty = 0,
                            DivertLane = divertLane.DivertLanes,
                            CurrentTs = currentTsSystem,
                            Status = "IN",
                            SAPSystem = sapSystemSAP,
                            Count = 0
                        };

                        await Context.AddAsync(boxAddDto);
                        await Context.SaveChangesAsync();
                        await AddScanLog(boxAddDto);
                    }
                }
                else
                {
                    return new WCSRoutingV10();
                }
                await connection.CloseAsync();
                var boxInfoResultAdd = await (from bx in Context.wCSRoutingV10s
                                              where bx.BoxId == boxId
                                              select bx).FirstOrDefaultAsync() ?? null;
                return boxInfoResultAdd;
            };
        }

        private async Task<bool> DivertLaneGeneralStatus(int divLane, string containerId)
        {
            var containExist = await Context.Containers.Where(c => c.ContainerId == containerId).Select(c => c.Id).FirstOrDefaultAsync();
            var divExist = await Context.DivertLanes.Where(dl => dl.DivertLanes == divLane && dl.ContainerId == containExist).Select(dl => dl).FirstOrDefaultAsync() ?? null;

            if (divExist != null)
            {
                if (divExist.Status == true && divExist.Full == false)
                {
                    return true;
                }
            }
            return false;
        }

        private async Task<bool> DivertLaneGeneralStatus2(int divLane)
        {
            var divExist = await Context.DivertLanes.Where(dl => dl.DivertLanes == divLane).Select(dl => dl).FirstOrDefaultAsync() ?? null;

            if (divExist != null)
            {
                if (divExist.Status == true && divExist.Full == false)
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<DivertBoxRespDto> DivertBox(string boxId, int trackingId)
        {
            var DivertResp = new DivertBoxRespDto();

            #region Hospital Lane
            if (boxId == "?" || boxId == "111111")//Para hospital lane
            {
                var divLaneStatus = await DivertLaneGeneralStatus2(32);

                if (divLaneStatus)
                {
                    DivertResp.DivertCode = 32;
                    DivertResp.TrackingId = trackingId;
                    DivertResp.BoxId = boxId;
                    return DivertResp;
                }
            }
            #endregion
            else
            {
                var boxLocalRegister = await (from r in Context.wCSRoutingV10s
                                              where r.BoxId == boxId && r.Status == "IN"
                                              select r).FirstOrDefaultAsync() ?? null;

                if (boxLocalRegister != null)
                {
                    boxLocalRegister.TrackingId = trackingId;
                    boxLocalRegister.DivertTs = DateTime.Now.ToString("yyyyMMddHHmmssfff");

                    Context.wCSRoutingV10s.Update(boxLocalRegister);
                    //await AddScanLog(boxLocalRegister);
                    await Context.SaveChangesAsync();

                    var recirculationLimitValue = await Context.RecirculationLimits.FirstOrDefaultAsync();
                    if (boxLocalRegister.DivertLane == 999)
                    {
                        if (boxLocalRegister.Count >= recirculationLimitValue?.CountLimit && boxLocalRegister.ContainerType == "S")
                        {
                            if (!string.IsNullOrEmpty(boxLocalRegister.ConfirmationNumber))
                            {
                                var isWaveRegistered = await Context.multiBox_Wave.Where(mb => mb.ConfirmationNumber == boxLocalRegister.ConfirmationNumber).Select(mb => mb).OrderByDescending(mb => mb.Id).FirstOrDefaultAsync() ?? null;

                                if (isWaveRegistered != null && isWaveRegistered.Status == null )
                                {
                                    isWaveRegistered.QtyCount--;
                                    Context.Update(isWaveRegistered);

                                    boxLocalRegister.DivertLane = 30;
                                    boxLocalRegister.ContainerType = "P";
                                    boxLocalRegister.Count = 0;
                                    boxLocalRegister.ConfirmationNumber = "";
                                    boxLocalRegister.ContainerId = "";
                                    Context.wCSRoutingV10s.Update(boxLocalRegister);
                                    await Context.SaveChangesAsync();
                                    await AddScanLog(boxLocalRegister);

                                    DivertResp.DivertCode = boxLocalRegister.DivertLane;
                                    DivertResp.TrackingId = trackingId;
                                    DivertResp.BoxId = boxId;
                                    return DivertResp;
                                }
                                else
                                {
                                    boxLocalRegister.DivertLane = 30;
                                    boxLocalRegister.ContainerType = "P";
                                    boxLocalRegister.Count = 0;
                                    Context.wCSRoutingV10s.Update(boxLocalRegister);
                                    await Context.SaveChangesAsync();
                                    await AddScanLog(boxLocalRegister);
                                    DivertResp.DivertCode = boxLocalRegister.DivertLane;
                                    DivertResp.TrackingId = trackingId;
                                    DivertResp.BoxId = boxId;
                                    return DivertResp;
                                }
                            }
                            else
                            {
                                boxLocalRegister.DivertLane = 30;
                                boxLocalRegister.ContainerType = "P";
                                boxLocalRegister.Count = 0;
                                Context.wCSRoutingV10s.Update(boxLocalRegister);
                                await Context.SaveChangesAsync();
                                await AddScanLog(boxLocalRegister);
                                DivertResp.DivertCode = boxLocalRegister.DivertLane;
                                DivertResp.TrackingId = trackingId;
                                DivertResp.BoxId = boxId;
                                return DivertResp;
                            }
                        }

                        DivertResp.DivertCode = 99;
                        DivertResp.TrackingId = trackingId;
                        DivertResp.BoxId = boxId;
                        await AddScanLog(boxLocalRegister);
                        return DivertResp;
                    }
                    else if(boxLocalRegister.Count >= recirculationLimitValue?.CountLimit)
                    {
                        boxLocalRegister.DivertLane = 30;
                        boxLocalRegister.ContainerType = "P";
                        boxLocalRegister.Count = 0;
                        Context.wCSRoutingV10s.Update(boxLocalRegister);
                        await Context.SaveChangesAsync();
                        await AddScanLog(boxLocalRegister);

                        DivertResp.DivertCode = boxLocalRegister.DivertLane;
                        DivertResp.TrackingId = trackingId;
                        DivertResp.BoxId = boxId;
                        return DivertResp;
                    }

                    bool divertLaneStatus;
                    if (boxLocalRegister.ContainerType == "P")
                    {
                        divertLaneStatus = await DivertLaneGeneralStatus2(30);
                    }
                    else
                    {
                        divertLaneStatus = await DivertLaneGeneralStatus2(boxLocalRegister.DivertLane);
                    }

                    if (divertLaneStatus)
                    {

                        var containerId = await Context.Containers.Where(c => c.ContainerId == boxLocalRegister.ContainerId).Select(c => c).FirstOrDefaultAsync() ?? null;

                        DivertLane containerExistinDivertLane = null;

                        if (containerId != null)
                        {
                            containerExistinDivertLane = await Context.DivertLanes.Where(dl => dl.DivertLanes == boxLocalRegister.DivertLane && dl.ContainerId == containerId.Id).Select(dl => dl).FirstOrDefaultAsync() ?? null;
                        }

                        #region Multiboxing
                        if (boxLocalRegister.ContainerType == "T" && (boxLocalRegister.ConfirmationNumber != null || boxLocalRegister.ConfirmationNumber?.Length > 0))
                        {
                            var isWaveRegistered = await _multiBoxWaveRepository.GetMultiBoxWaveAsync(boxLocalRegister.ConfirmationNumber);

                            if (isWaveRegistered != null && isWaveRegistered.Status == "IN" && (containerExistinDivertLane != null && (containerId?.ContainerId != isWaveRegistered.ContainerId || boxLocalRegister.DivertLane != isWaveRegistered.DivertLane)))
                            {
                                boxLocalRegister.DivertLane = 30;
                                boxLocalRegister.Count = 0;
                                boxLocalRegister.ContainerType = "P";
                                Context.wCSRoutingV10s.Update(boxLocalRegister);
                                await Context.SaveChangesAsync();
                                await AddScanLog(boxLocalRegister);

                                DivertResp.DivertCode = boxLocalRegister.DivertLane;
                                DivertResp.TrackingId = trackingId;
                                DivertResp.BoxId = boxId;
                                return DivertResp;
                            }
                        }
                        #endregion

                        if ((containerId == null || containerExistinDivertLane == null) && (boxLocalRegister.ContainerType == "G" || boxLocalRegister.ContainerType == "T"))
                        {
                            /*
                            Por si es necesario reducir los tiempos de vueltas de cajas
                            Context.wCSRoutingV10s.Remove(boxLocalRegister);
                            await Context.SaveChangesAsync();

                            var res = await GetByBoxId(boxId, trackingId);

                            DivertResp.DivertCode = res.DivertLane;
                            DivertResp.TrackingId = (int)res.TrackingId;
                            DivertResp.BoxId = res.BoxId;
                            return DivertResp;
                            */


                            await AddScanLog(boxLocalRegister);
                            Context.wCSRoutingV10s.Remove(boxLocalRegister);
                            await Context.SaveChangesAsync();

                            DivertResp.DivertCode = 99;
                            DivertResp.TrackingId = trackingId;
                            DivertResp.BoxId = boxId;
                            return DivertResp;
                        }

                        await AddScanLog(boxLocalRegister);
                        DivertResp.DivertCode = boxLocalRegister.DivertLane;
                        DivertResp.TrackingId = trackingId;
                        DivertResp.BoxId = boxId;
                        return DivertResp;
                    }
                    else
                    {

                        if (boxLocalRegister.Count >= recirculationLimitValue?.CountLimit && (boxLocalRegister.ContainerType == "G" || boxLocalRegister.ContainerType == "T"))
                        {
                            boxLocalRegister.DivertLane = 30;
                            boxLocalRegister.ContainerType = "P";
                            boxLocalRegister.Count = 0;
                            Context.wCSRoutingV10s.Update(boxLocalRegister);
                            await AddScanLog(boxLocalRegister);
                            await Context.SaveChangesAsync();

                            DivertResp.DivertCode = boxLocalRegister.DivertLane;
                            DivertResp.TrackingId = trackingId;
                            DivertResp.BoxId = boxId;
                            return DivertResp;
                        }

                        var containerId = await Context.Containers.Where(c => c.ContainerId == boxLocalRegister.ContainerId).Select(c => c).FirstOrDefaultAsync() ?? null;

                        DivertLane containerExistinDivertLane = null;

                        if (containerId != null)
                        {
                            containerExistinDivertLane = await Context.DivertLanes.Where(dl => dl.DivertLanes == boxLocalRegister.DivertLane && dl.ContainerId == containerId.Id).Select(dl => dl).FirstOrDefaultAsync() ?? null;
                        }

                        #region Multiboxing
                        if (boxLocalRegister.ContainerType == "T" && (boxLocalRegister.ConfirmationNumber != null || boxLocalRegister.ConfirmationNumber?.Length > 0))
                        {
                            var isWaveRegistered = await _multiBoxWaveRepository.GetMultiBoxWaveAsync(boxLocalRegister.ConfirmationNumber);

                            if (isWaveRegistered != null && isWaveRegistered.Status == "IN" && (containerExistinDivertLane != null && containerId?.ContainerId == isWaveRegistered.ContainerId && boxLocalRegister.DivertLane == isWaveRegistered.DivertLane))
                            {
                                boxLocalRegister.DivertLane = 30;
                                boxLocalRegister.Count = 0;
                                boxLocalRegister.ContainerType = "P";
                                Context.wCSRoutingV10s.Update(boxLocalRegister);
                                await Context.SaveChangesAsync();
                                await AddScanLog(boxLocalRegister);

                                DivertResp.DivertCode = boxLocalRegister.DivertLane;
                                DivertResp.TrackingId = trackingId;
                                DivertResp.BoxId = boxId;
                                return DivertResp;
                            }
                            if (isWaveRegistered != null && isWaveRegistered.Status == "IN" && (containerExistinDivertLane == null && boxLocalRegister.DivertLane == isWaveRegistered.DivertLane && divertLaneStatus == false))
                            {
                                boxLocalRegister.DivertLane = 30;
                                boxLocalRegister.Count = 0;
                                boxLocalRegister.ContainerType = "P";
                                Context.wCSRoutingV10s.Update(boxLocalRegister);
                                await Context.SaveChangesAsync();
                                await AddScanLog(boxLocalRegister);

                                DivertResp.DivertCode = boxLocalRegister.DivertLane;
                                DivertResp.TrackingId = trackingId;
                                DivertResp.BoxId = boxId;
                                return DivertResp;
                            }
                        }
                        #endregion

                        if ((containerId == null || containerExistinDivertLane == null) && (boxLocalRegister.ContainerType == "G" || boxLocalRegister.ContainerType == "T"))
                        {
                            /*
                            Por si es necesario reducir los tiempos de vueltas de cajas
                            Context.wCSRoutingV10s.Remove(boxLocalRegister);
                            await Context.SaveChangesAsync();

                            var res = await GetByBoxId(boxId, trackingId);

                            DivertResp.DivertCode = res.DivertLane;
                            DivertResp.TrackingId = (int)res.TrackingId;
                            DivertResp.BoxId = res.BoxId;
                            return DivertResp;
                            */

                            await AddScanLog(boxLocalRegister);
                            Context.wCSRoutingV10s.Remove(boxLocalRegister);

                            await Context.SaveChangesAsync();

                            DivertResp.DivertCode = 99;
                            DivertResp.TrackingId = trackingId;
                            DivertResp.BoxId = boxId;
                            return DivertResp;
                        }
                    }
                }
                else
                {
                    DivertResp.DivertCode = 30;
                    DivertResp.TrackingId = trackingId;
                    DivertResp.BoxId = boxId;
                    return DivertResp;
                }
            }

            DivertResp.DivertCode = 99;
            DivertResp.TrackingId = trackingId;
            DivertResp.BoxId = boxId;
            return DivertResp;
        }

        public async Task<bool> DivertConfirmBox(string div_timestamp, int trackingId)
        {

            DateTime div_timestampPLC = DateTime.ParseExact(div_timestamp, "yyyyMMddHHmmssfff", null);
            var minDivTimestampStr = div_timestampPLC.AddSeconds(-38).ToString("yyyyMMddHHmmssfff");
            var maxDivTimestampStr = div_timestampPLC.ToString("yyyyMMddHHmmssfff");

            var result = await Context.wCSRoutingV10s
                .Where(ti => string.Compare(ti.DivertTs, minDivTimestampStr) >= 0 && string.Compare(ti.DivertTs, maxDivTimestampStr) <= 0 && ti.TrackingId == trackingId && ti.Status == "IN")
                .FirstOrDefaultAsync() ?? null;


            if (result != null)
            {
                #region MultiBoxWave
                if (result.ContainerType == "T")
                {
                    if (!string.IsNullOrEmpty(result.ConfirmationNumber))
                    {
                        var existeRegistro = await Context.multiBox_Wave.Where(x => x.ConfirmationNumber == result.ConfirmationNumber).FirstOrDefaultAsync() ?? null;

                        if (existeRegistro.Status == null || existeRegistro.Status == "NA")
                        {
                            return false;
                        }

                        if (existeRegistro.Status == "IN")
                        {
                            existeRegistro.QtyCount++;
                            if (existeRegistro.QtyCount < existeRegistro.Qty)
                            {
                                Context.Update(existeRegistro);
                                await Context.SaveChangesAsync();
                            }
                            else if (existeRegistro.QtyCount == existeRegistro.Qty)
                            {
                                existeRegistro.Status = "NA";
                                Context.Update(existeRegistro);
                                await Context.SaveChangesAsync();
                            }
                        }
                    }
                }

                #endregion

                bool divLaneStatus;
                if (result.ContainerType == "P")
                {
                    divLaneStatus = await DivertLaneGeneralStatus2(result.DivertLane);
                }
                else
                {
                    divLaneStatus = await DivertLaneGeneralStatus(result.DivertLane, result.ContainerId);
                }


                if (!divLaneStatus)
                {
                    return false;
                }

                result.TrackingId = 0;
                result.Status = "NA";
                await AddScanLog(result);
                await Context.SaveChangesAsync();


                #region Insert Data in Table Border only ContainerType stars with T o G
                if (result.ContainerType == "G" || result.ContainerType == "T")
                {
                    var insertQuery = $@"";
                    if (result.ContainerType == "G")
                    {
                        insertQuery = $@"
                                INSERT INTO WCS_Routing (
                                BoxId, BoxType, CarrierCode, LogisticAgent, ConfirmationNumber, 
                                ContainerId, ContainerType, Qty, DivertLane, CurrentTs, 
                                Status, SAPSystem
                                )
                                VALUES (
                                '{result.BoxId}', '{result.BoxType}', '{result.CarrierCode}', '{result.LogisticAgent}', '{result.ConfirmationNumber}', 
                                '{result.ContainerId}', 'H', 0, {result.DivertLane}, '{result.CurrentTs}', 
                                'IN', '{result.SAPSystem}'
                                )";
                        await AddScanLog(result);
                    }
                    else
                    if (result.ContainerType == "T")
                    {//NOTA CHECAR SI CAMNBIAR EL QTY DE 0 AL VALOR QUE TENGA EL REGISTRO
                        insertQuery = $@"
                                INSERT INTO WCS_Routing (
                                BoxId, BoxType, CarrierCode, LogisticAgent, ConfirmationNumber, 
                                ContainerId, ContainerType, Qty, DivertLane, CurrentTs, 
                                Status, SAPSystem
                                )
                                VALUES (
                                '{result.BoxId}', '{result.BoxType}', '{result.CarrierCode}', '{result.LogisticAgent}', '{result.ConfirmationNumber}', 
                                '{result.ContainerId}', 'I', 0, {result.DivertLane}, '{result.CurrentTs}',
                                'IN', '{result.SAPSystem}'
                                )";
                        await AddScanLog(result);

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
                                '{result.BoxId}', '{result.BoxType}', '{result.CarrierCode}', '{result.LogisticAgent}', '{result.ConfirmationNumber}', 
                                '{result.ContainerId}', '{result.ContainerType}', 0, {result.DivertLane}, '{result.CurrentTs}', 
                                'IN', '{result.SAPSystem}'
                                )";
                        await AddScanLog(result);

                    }
                    await _sapContext.Database.ExecuteSqlRawAsync(insertQuery);
                    await _sapContext.SaveChangesAsync();
                }
                #endregion
                return true;
            }
            return false;
        }


        private async Task<WCSRoutingV10> AddBoxDtoValuePalletAsync(string boxId, string boxTypeSAP, string carrierCodeSAP, string logisticAgentSAP, string confirmationNumberSAP, string currentTsSystem, string sapSystemSAP)
        {
            WCSRoutingV10 boxAddDtoValuePallet = new WCSRoutingV10
            {
                BoxId = boxId,
                BoxType = boxTypeSAP,
                CarrierCode = carrierCodeSAP,
                LogisticAgent = logisticAgentSAP,
                ConfirmationNumber = confirmationNumberSAP,
                ContainerId = "",
                ContainerType = "P",
                Qty = 0,
                DivertLane = 30,
                CurrentTs = currentTsSystem,
                Status = "IN",
                SAPSystem = sapSystemSAP,
                Count = 0
            };
            await Context.AddAsync(boxAddDtoValuePallet);
            await AddScanLog(boxAddDtoValuePallet);
            await Context.SaveChangesAsync();

            var boxInfoResultAddPallet = await (from bx in Context.wCSRoutingV10s
                                                where bx.BoxId == boxId
                                                select bx).FirstOrDefaultAsync() ?? null;



            return boxInfoResultAddPallet;
        }
        private async Task<WCSRoutingV10> AddBoxDtoValuePalletForMultiAsync(string boxId, string boxTypeSAP, string carrierCodeSAP, string logisticAgentSAP, string confirmationNumberSAP, string containerId, string currentTsSystem, string sapSystemSAP)
        {
            WCSRoutingV10 boxAddDtoValuePallet = new WCSRoutingV10
            {
                BoxId = boxId,
                BoxType = boxTypeSAP,
                CarrierCode = carrierCodeSAP,
                LogisticAgent = logisticAgentSAP,
                ConfirmationNumber = confirmationNumberSAP,
                ContainerId = containerId,
                ContainerType = "P",
                Qty = 0,
                DivertLane = 30,
                CurrentTs = currentTsSystem,
                Status = "IN",
                SAPSystem = sapSystemSAP,
                Count = 0
            };
            await Context.AddAsync(boxAddDtoValuePallet);
            await AddScanLog(boxAddDtoValuePallet);
            await Context.SaveChangesAsync();

            var boxInfoResultAddPallet = await (from bx in Context.wCSRoutingV10s
                                                where bx.BoxId == boxId
                                                select bx).FirstOrDefaultAsync() ?? null;



            return boxInfoResultAddPallet;
        }

        private async Task<WCSRoutingV10> MetodoParaRegistrarCajaSinDestino(string boxId, string boxTypeSAP, string carrierCodeSAP, string logisticAgentSAP, string confirmationNumberSAP, string currentTsSystem, string sapSystemSAP)
        {
            WCSRoutingV10 boxAddDtoValuePallet = new WCSRoutingV10
            {
                BoxId = boxId,
                BoxType = boxTypeSAP,
                CarrierCode = carrierCodeSAP,
                LogisticAgent = logisticAgentSAP,
                ConfirmationNumber = confirmationNumberSAP,
                ContainerId = "",
                ContainerType = "S",
                Qty = 0,
                DivertLane = 999,
                CurrentTs = currentTsSystem,
                Status = "IN",
                SAPSystem = sapSystemSAP,
                Count = 0
            };

            await Context.AddAsync(boxAddDtoValuePallet);
            await AddScanLog(boxAddDtoValuePallet);
            await Context.SaveChangesAsync();

            var boxInfoResultAddPallet = await (from bx in Context.wCSRoutingV10s
                                                where bx.BoxId == boxId
                                                select bx).FirstOrDefaultAsync() ?? null;

            return boxInfoResultAddPallet;
        }
        private async Task<WCSRoutingV10> MetodoParaRegistrarCajaMultiboxSinDestino(string boxId, string boxTypeSAP, string carrierCodeSAP, string logisticAgentSAP, string confirmationNumberSAP, string currentTsSystem, string sapSystemSAP, string? containerId, int qty)
        {
            WCSRoutingV10 boxAddDtoValuePallet = new WCSRoutingV10
            {
                BoxId = boxId,
                BoxType = boxTypeSAP,
                CarrierCode = carrierCodeSAP,
                LogisticAgent = logisticAgentSAP,
                ConfirmationNumber = confirmationNumberSAP,
                ContainerId = containerId is null ? "" : containerId,
                ContainerType = "S",
                Qty = qty,
                DivertLane = 999,
                CurrentTs = currentTsSystem,
                Status = "IN",
                SAPSystem = sapSystemSAP,
                Count = 0
            };

            await Context.AddAsync(boxAddDtoValuePallet);
            await AddScanLog(boxAddDtoValuePallet);
            await Context.SaveChangesAsync();

            var boxInfoResultAddPallet = await (from bx in Context.wCSRoutingV10s
                                                where bx.BoxId == boxId
                                                select bx).FirstOrDefaultAsync() ?? null;

            return boxInfoResultAddPallet;
        }


        private static int NextDivertLaneId(List<DivertLane> divertLaneResult, int index)
        {
            int nextIndex = (index + 1) % divertLaneResult.Count;

            return divertLaneResult[nextIndex].Id;
        }

        public async Task AddScanLogHospital(string boxId, int d)
        {
            try
            {
                var newScanlog = new ScanLogSorting
                {
                    BoxId = boxId,
                    ContainerType = "HOSPITAL",
                    DivertLane = d,
                    Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff")
                };
                await Context.scanLogSortings.AddAsync(newScanlog);
                await Context.SaveChangesAsync();
            }
            catch { }
        }

        public async Task AddScanLog(WCSRoutingV10 data)
        {
            try
            {
                var scanLog = new ScanLogSorting();

                scanLog.BoxId = data.BoxId;
                scanLog.Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                scanLog.CarrierCode = data.CarrierCode;
                scanLog.LogisticAgent = data.LogisticAgent;
                scanLog.BoxType = data.BoxType;
                scanLog.ContainerId = data.ContainerId;
                scanLog.DivertLane = data.DivertLane;

                scanLog.ConfirmationNumber = data.ConfirmationNumber;

                scanLog.Count = data.Count;
                scanLog.Qty = data.Qty;
                scanLog.Status = data.Status;
                scanLog.TrackingId = data.TrackingId ?? 0;

                switch (data.ContainerType)
                {
                    case "G":
                        scanLog.ContainerType = "H";
                        break;
                    case "T":
                        scanLog.ContainerType = "I";
                        break;
                    case "P":
                        scanLog.ContainerType = "Pallete";
                        break;
                    case "H":
                        scanLog.ContainerType = "Hospital";
                        break;
                    default:
                        break;
                }


                await Context.scanLogSortings.AddAsync(scanLog);
                await Context.SaveChangesAsync();
            }
            catch{ }
        }
    }
}
