using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodeDivertLaneMapping;
using LuxotticaSorting.ApplicationServices.Shared.DTO.ZebraHistorial;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.DataAccess.Repositories.ZebraHistorial
{
    public class ZebraHistorialRepository : Repository<int, Core.Zebra.ZebraHistorial>
    {
        public ZebraHistorialRepository(SortingContext context) : base(context)
        {

        }

        public async Task<List<Core.Zebra.ZebraHistorial>> DeleteAsync()
        {

            var entitiesToDelete = await Context.ZebraHistorials
                                               .Where(z => z.Status == true)
                                               .ToListAsync();

            if (entitiesToDelete == null)
            {
                throw new Exception($"No Zebra Historial records with a successfull status.");
            }

            // Eliminar los registros encontrados
            Context.ZebraHistorials.RemoveRange(entitiesToDelete);
            await Context.SaveChangesAsync();

            return entitiesToDelete;
        }

        public async Task<List<ZebraHistorialData>> GetReprintGaylordAsync()
        {
            var entitiesToShow = (from mapping in Context.ZebraHistorials
                                    join divertlanesmapping in Context.DivertLanes on mapping.DivertLaneId equals divertlanesmapping.Id
                                    join container in Context.Containers on mapping.ContainerId equals container.Id
                                    join printer in Context.ZebraConfigurations on mapping.ZebraConfigurationId equals printer.Id
                                  orderby mapping.Timestamp descending
                                  select new ZebraHistorialData
                                    {
                                        Id = mapping.Id,
                                        DivertLaneId = mapping.DivertLaneId,
                                        DivertLanes = divertlanesmapping.DivertLanes,
                                        ContainerId = mapping.ContainerId,
                                        ContainerValue = container.ContainerId,
                                        ZebraConfigurationId = mapping.ContainerId,
                                        ZebraConfigurationName = printer.NamePrinter,
                                        Timestamp = mapping.Timestamp,
                                        Status = mapping.Status
                                    }).Where(z => z.Status == false && z.ContainerValue.StartsWith("GLD") && z.ContainerValue.Length == 20).ToList();

            if (entitiesToShow == null)
            {
                throw new Exception($"No Zebra Historial records Gaylord to Reprint.");
            }

            return entitiesToShow;
        }

        public async Task<List<ZebraHistorialData>> GetGaylordAsync()
        {
            var entitiesToShow = (from mapping in Context.ZebraHistorials
                                  join divertlanesmapping in Context.DivertLanes on mapping.DivertLaneId equals divertlanesmapping.Id
                                  join container in Context.Containers on mapping.ContainerId equals container.Id
                                  join printer in Context.ZebraConfigurations on mapping.ZebraConfigurationId equals printer.Id
                                  orderby mapping.Timestamp descending
                                  select new ZebraHistorialData
                                  {
                                      Id = mapping.Id,
                                      DivertLaneId = mapping.DivertLaneId,
                                      DivertLanes = divertlanesmapping.DivertLanes,
                                      ContainerId = mapping.ContainerId,
                                      ContainerValue = container.ContainerId,
                                      ZebraConfigurationId = mapping.ContainerId,
                                      ZebraConfigurationName = printer.NamePrinter,
                                      Timestamp = mapping.Timestamp,
                                      Status = mapping.Status
                                  }).Where(z => z.ContainerValue.StartsWith("GLD") && z.ContainerValue.Length == 20).ToList();

            if (entitiesToShow == null)
            {
                throw new Exception($"No Zebra Historial Gaylord registered.");
            }

            return entitiesToShow;
        }

        public async Task<List<ZebraHistorialData>> GetCombinatedDataAsync()
        {
            var entitiesToShow = (from mapping in Context.ZebraHistorials
                                  join divertlanesmapping in Context.DivertLanes on mapping.DivertLaneId equals divertlanesmapping.Id
                                  join container in Context.Containers on mapping.ContainerId equals container.Id
                                  join printer in Context.ZebraConfigurations on mapping.ZebraConfigurationId equals printer.Id
                                  orderby mapping.Timestamp descending
                                  select new ZebraHistorialData
                                  {
                                      Id = mapping.Id,
                                      DivertLaneId = mapping.DivertLaneId,
                                      DivertLanes = divertlanesmapping.DivertLanes,
                                      ContainerId = mapping.ContainerId,
                                      ContainerValue = container.ContainerId,
                                      ZebraConfigurationId = mapping.ContainerId,
                                      ZebraConfigurationName = printer.NamePrinter,
                                      Timestamp = mapping.Timestamp,
                                      Status = mapping.Status
                                  }).ToList();

            if (entitiesToShow == null)
            {
                throw new Exception($"No Zebra Historial records.");
            }

            return entitiesToShow;
        }

        public async Task<List<ZebraHistorialData>> GetTruckAsync()
        {
            var entitiesToShow = (from mapping in Context.ZebraHistorials
                                  join divertlanesmapping in Context.DivertLanes on mapping.DivertLaneId equals divertlanesmapping.Id
                                  join container in Context.Containers on mapping.ContainerId equals container.Id
                                  join printer in Context.ZebraConfigurations on mapping.ZebraConfigurationId equals printer.Id
                                  orderby mapping.Timestamp descending
                                  select new ZebraHistorialData
                                  {
                                      Id = mapping.Id,
                                      DivertLaneId = mapping.DivertLaneId,
                                      DivertLanes = divertlanesmapping.DivertLanes,
                                      ContainerId = mapping.ContainerId,
                                      ContainerValue = container.ContainerId,
                                      ZebraConfigurationId = mapping.ContainerId,
                                      ZebraConfigurationName = printer.NamePrinter,
                                      Timestamp = mapping.Timestamp,
                                      Status = mapping.Status
                                  }).Where(z => z.ContainerValue.StartsWith("1") && z.ContainerValue.Length == 10).ToList();
            if (entitiesToShow == null)
            {
                throw new Exception($"No Zebra Historial Trucks records.");
            }

            return entitiesToShow;
        }

        public async Task<List<ZebraHistorialData>> GetTruckToReprintAsync()
        {
            var entitiesToShow = (from mapping in Context.ZebraHistorials
                                  join divertlanesmapping in Context.DivertLanes on mapping.DivertLaneId equals divertlanesmapping.Id
                                  join container in Context.Containers on mapping.ContainerId equals container.Id
                                  join printer in Context.ZebraConfigurations on mapping.ZebraConfigurationId equals printer.Id
                                  orderby mapping.Timestamp descending
                                  select new ZebraHistorialData
                                  {
                                      Id = mapping.Id,
                                      DivertLaneId = mapping.DivertLaneId,
                                      DivertLanes = divertlanesmapping.DivertLanes,
                                      ContainerId = mapping.ContainerId,
                                      ContainerValue = container.ContainerId,
                                      ZebraConfigurationId = mapping.ContainerId,
                                      ZebraConfigurationName = printer.NamePrinter,
                                      Timestamp = mapping.Timestamp,
                                      Status = mapping.Status
                                  }).Where(z => z.ContainerValue.StartsWith("1") && z.ContainerValue.Length == 10 && z.Status == false).ToList();
            if (entitiesToShow == null)
            {
                throw new Exception($"No Zebra Historial Trucks records.");
            }

            return entitiesToShow;
        }

        public override IQueryable<Core.Zebra.ZebraHistorial> GetAll()
        {
            var zebraHistorial = from zebraHtry in Context.ZebraHistorials
                                 orderby zebraHtry.Timestamp descending
                                 select zebraHtry;
            return zebraHistorial;
        }
    }
}
