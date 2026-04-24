using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.Shared.DTO.MultiBoxWaves;
using LuxotticaSorting.Core.DivertLanes;
using LuxotticaSorting.Core.MultiBoxWaves;
using LuxotticaSorting.Core.WCSRoutingV10;
using LuxotticaSorting.DataAccess.Repositories.RoutingV10S;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LuxotticaSorting.DataAccess.Repositories.MultiBoxWaves
{
    public class MultiBoxWaveRepository : Repository<int, MultiBoxWave>
    {
        private readonly SAPContext _sapContext;
        private IConfiguration Configuration { get; }
        public MultiBoxWaveRepository(SortingContext context, SAPContext sapContext, IConfiguration configuration) : base(context)
        {
            _sapContext = sapContext;
            Configuration = configuration;
        }

        public async Task<int?> MaxCountQtyConfiguration(int NewCount)
        {
            try
            {
                var updateQtyFromConfiguration = $@"
                                    UPDATE [ConfigurationsSetting] SET MaxBoxCount = {NewCount}";
                await _sapContext.Database.ExecuteSqlRawAsync(updateQtyFromConfiguration);

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
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<int?> GetMaxCountQtyConfiguration()
        {
            try
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
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<(int, bool)> AddMultiBoxWaveAsync(MultiBoxWave entity)
        {
            try
            {
                var existingRecord = await Context.Set<MultiBoxWave>()
                .Where(mbw => mbw.ContainerId == entity.ContainerId
                              && mbw.ContainerType == entity.ContainerType
                              && mbw.DivertLane == entity.DivertLane
                              && mbw.ConfirmationNumber == entity.ConfirmationNumber)
                .FirstOrDefaultAsync();

                var divertLaneRecord = await Context.Set<DivertLane>()
                .Include(dl => dl.Container)
                .Where(dl => dl.DivertLanes == entity.DivertLane
                             && dl.Container.ContainerId == entity.ContainerId
                             && dl.Container.ContainerType.ContainerTypes == entity.ContainerType)
                .FirstOrDefaultAsync();

                if (divertLaneRecord == null)
                {
                    return (0, false);
                }

                if (existingRecord != null)
                {
                    // Ya existe un registro
                    existingRecord.QtyCount++;

                    if (existingRecord.QtyCount == existingRecord.Qty)
                    {
                        existingRecord.Status = "IN";
                        existingRecord.QtyCount = 0;
                    }

                    await UpdateAsync(existingRecord);
                    await Context.SaveChangesAsync();
                    // Retorna (0, true) indicando que no se agregó un nuevo registro pero la operación fue exitosa
                    return (0, true);
                }
                else
                {
                    // No existe registro
                    await AddAsync(entity);
                    entity.QtyCount = 1;
                    await Context.SaveChangesAsync();
                    // Retorna (1, true) indicando que se agregó un nuevo registro y la operación fue exitosa
                    return (1, true);
                }
            }
            catch
            {
                return (0, false);
            }
        }

        public async Task<(int, bool)> ConfirmMultiBoxWaveAsync(MultiBoxWave entity)
        {
            try
            {
                var existingRecord = await Context.Set<MultiBoxWave>()
                .Where(mbw => mbw.ContainerId == entity.ContainerId
                              && mbw.ContainerType == entity.ContainerType
                              && mbw.DivertLane == entity.DivertLane
                              && mbw.ConfirmationNumber == entity.ConfirmationNumber)
                .FirstOrDefaultAsync();

                if (existingRecord != null)
                {
                    if (existingRecord.Status == "IN")
                    {
                        existingRecord.QtyCount++;

                        if (existingRecord.QtyCount == existingRecord.Qty)
                        {
                            existingRecord.Status = "NA";
                        }

                        await UpdateAsync(existingRecord);
                        await Context.SaveChangesAsync();

                        return (0, true);
                    }
                    return (0, false);
                }
                else
                {
                    return (0, false);
                }
            }
            catch
            {
                return (0, false);
            }
        }

        public async Task<bool> UpdateWaveCountForWaveNull(string confirmationNumber)
        {
            try
            {
                var waveUpdated = await Context.multiBox_Wave.Where(mb => mb.ConfirmationNumber == confirmationNumber).Select(mb => mb).FirstOrDefaultAsync();

                if (waveUpdated != null)
                {
                    if (waveUpdated.Status == null)
                    {
                        waveUpdated.QtyCount++;

                        if (waveUpdated.QtyCount == waveUpdated.Qty)
                        {
                            waveUpdated.Status = "IN";
                            waveUpdated.QtyCount = 0;
                        }

                        Context.multiBox_Wave.Update(waveUpdated);
                        await Context.SaveChangesAsync();

                        return true;
                    }
                    return false;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<MultiBoxWavesGetDto> GetMultiBoxWaveAsync(string confirmationNumber)
        {
            try
            {
                var multiBoxWaveEntity = await Context.multiBox_Wave
                    .FirstOrDefaultAsync(mbw => mbw.ConfirmationNumber == confirmationNumber);


                if (multiBoxWaveEntity != null)
                {
                    var multiBoxWaveDto = new MultiBoxWavesGetDto
                    {
                        ContainerId = multiBoxWaveEntity.ContainerId,
                        ContainerType = multiBoxWaveEntity.ContainerType,
                        DivertLane = multiBoxWaveEntity.DivertLane,
                        ConfirmationNumber = multiBoxWaveEntity.ConfirmationNumber,
                        Qty = multiBoxWaveEntity.Qty,
                        QtyCount = multiBoxWaveEntity.QtyCount,
                        Status = multiBoxWaveEntity.Status
                    };

                    return multiBoxWaveDto;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<MultiBoxWavesGetAllDto>> GetAllBoxMultiBoxWaveAsync()
        {
            try
            {

                var confirmationNumbers = await Context.multiBox_Wave.Where(x => x.Status == "IN" && (x.ContainerId != "CONTAINER2" && x.ContainerId != "CONTAINER4"))
                    .Select(mbw => mbw.ConfirmationNumber)
                    .ToListAsync();

                var multiBoxWavesDto = new List<MultiBoxWavesGetAllDto>();

                foreach (var confirmationNumber in confirmationNumbers)
                {
                    var wcsRoutingData = await Context.wCSRoutingV10s
                        .Where(w => w.ConfirmationNumber == confirmationNumber && (!string.IsNullOrWhiteSpace(w.ContainerId) || w.ContainerId != "CONTAINER2" || w.ContainerId != "CONTAINER4"))
                        .GroupBy(w => w.BoxId)
                        .Select(group => group.OrderByDescending(w => w.DivertTs).FirstOrDefault())
                        .ToListAsync();

                    int? divertLane = wcsRoutingData.Any(w => w.DivertLane == 2) ? 2 :
                                      wcsRoutingData.Any(w => w.DivertLane == 4) ? 4 :
                                      wcsRoutingData.All(w => w.DivertLane == 30) ? 30 : (int?)null;

                    var groupedDto = wcsRoutingData
                        .GroupBy(wcsData => new { wcsData.ConfirmationNumber, wcsData.ContainerType })
                        .Select(group => new MultiBoxWavesGetAllDto
                        {
                            DivertLane = divertLane ?? 30,
                            ConfirmationNumber = group.Key.ConfirmationNumber,
                            Qty = group.First().Qty,
                            Status = group.First().Status,
                            StatusFront = group.Key.ContainerType,
                            Boxes = group.Select(wcs => new BoxInfoDto { BoxId = wcs.BoxId, Status = wcs.Status }).ToList()
                        });

                    multiBoxWavesDto.AddRange(groupedDto);
                }

                return multiBoxWavesDto;
            }
            catch
            {
                throw new Exception("Error getting data from MultiBoxWave.");
            }
        }

        public async Task<bool> ManualConfirmationMultiboxInWaves(string boxId, string confirmationNumber)
        {
            try
            {
                var result = await Context.wCSRoutingV10s.Where(x => x.BoxId == boxId && x.ConfirmationNumber == confirmationNumber
                        && (x.Status == "NA") & x.ContainerType == "P").FirstOrDefaultAsync();

                if (result != null)
                {
                    var wave = await Context.multiBox_Wave.Where(mw => mw.ConfirmationNumber == confirmationNumber).FirstOrDefaultAsync();
                    result.TrackingId = 0;
                    result.Status = "NA";
                    result.Count = 0;
                    result.ContainerType = wave.ContainerType;
                    result.DivertLane = wave.DivertLane;
                    result.Qty = wave.Qty;
                    result.ContainerId = wave.ContainerId;
                    await Context.SaveChangesAsync();

                    #region Insert counter in internal table 
                    var incrementQty = await Context.wCSRoutingV10s.Where(x => x.ContainerId == result.ContainerId && x.BoxId == null && x.BoxType == null
                        && x.CarrierCode == null && x.LogisticAgent == null && x.ConfirmationNumber == null).FirstOrDefaultAsync();

                    if (incrementQty != null)
                    {
                        incrementQty.Qty++;
                        Context.Update(incrementQty);
                        await Context.SaveChangesAsync();
                    }
                    else
                    {
                        await AddInternalTable(result);
                    }
                    #endregion

                    #region check in table MultiBox wave
                    var confirmationNumerComplete = await Context.multiBox_Wave.Where(x => x.ConfirmationNumber == confirmationNumber
                            && (x.Status == null || x.Status == "IN")).FirstOrDefaultAsync();
                    if (confirmationNumerComplete != null)
                    {
                        if (confirmationNumerComplete.QtyCount < confirmationNumerComplete.Qty)
                        {
                            confirmationNumerComplete.QtyCount++;
                        }

                        if (confirmationNumerComplete.QtyCount == confirmationNumerComplete.Qty)
                        {
                            confirmationNumerComplete.Status = "NA";
                        }
                        Context.Update(confirmationNumerComplete);
                        await Context.SaveChangesAsync();
                        return true;
                    }
                    return false;
                    #endregion
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                throw new Exception("An internal error has occurred.");
            }
        }

        public async Task AddInternalTable(WCSRoutingV10 result)
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
                    ContainerId = result.ContainerId,
                    ContainerType = result.ContainerType,
                    Qty = 1,
                    DivertLane = result.DivertLane,
                    CurrentTs = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    Status = "NA",
                    SAPSystem = result.SAPSystem,
                    DivertTs = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    TrackingId = 0,
                    Count = 0
                };

                Context.wCSRoutingV10s.Add(QtyRegister);
                await Context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
