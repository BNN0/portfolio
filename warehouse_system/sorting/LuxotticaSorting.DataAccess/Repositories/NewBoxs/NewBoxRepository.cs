using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.Shared.DTO.NewBoxs;
using LuxotticaSorting.Core.DivertLanes;
using LuxotticaSorting.Core.MappingSorter;
using LuxotticaSorting.Core.ScanLogSortings;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.DataAccess.Repositories.NewBoxs
{
    public class NewBoxRepository : Repository<int, NewBoxAddDto>
    {
        private SAPContext _configuration { get; }
        public NewBoxRepository(SortingContext context, SAPContext configuration) : base(context)
        {
           _configuration = configuration;
        }

        public override async Task<NewBoxAddDto> AddAsync(NewBoxAddDto newBox)
        {
            try
            {
                string currentTS = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                
                string insertQuery = $@"INSERT INTO SAP_Orders (BoxId, CarrierCode, LogisticAgent, BoxType, CurrentTS)
                           VALUES ('{newBox.BoxId}', '{newBox.CarrierCode}', '{newBox.LogisticAgent}', 
                           '{newBox.BoxType}', '{currentTS}')";

                _configuration.Database.ExecuteSqlRaw(insertQuery);
                await AddScanLog(newBox.BoxId, newBox.CarrierCode, newBox.LogisticAgent, newBox.BoxType);
                await _configuration.SaveChangesAsync();
                return newBox; 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al insertar en la base de datos: {ex.Message}");
                throw;
            }
        }

        public async Task AddScanLog(string boxId,string carrierCode,string logisticAgent,string boxType)
        {
            var newScanlog = new ScanLogSorting
            {
                BoxId = boxId,
                CarrierCode = carrierCode,
                LogisticAgent = logisticAgent,
                BoxType = boxType
            };
            await Context.scanLogSortings.AddAsync(newScanlog);
            await Context.SaveChangesAsync();
        }

    }
}
