using Luxottica.ApplicationServices.Shared.Dto.MapPhysicVirtualSAP;
using Luxottica.Core.Entities.PhysicalMaps;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Luxottica.DataAccess.Repositories.PhysicalMaps
{
    public class MapPhysicalVirtualSAPRepository : Repository<int, MapPhysicalVirtualSAP>
    {
        //private int? jackpotDivertLineTemp = null; 
        public MapPhysicalVirtualSAPRepository(V10Context context) : base(context)
        {
        }


        public override async Task<MapPhysicalVirtualSAP> AddAsync(MapPhysicalVirtualSAP mapPhysical)
        {
            var jackpotDivertLine = (from j in Context.JackpotLines where j.JackpotLineValue == true select j.DivertLineId).FirstOrDefault();
            if (jackpotDivertLine == null)
            {
                throw new InvalidOperationException("There is no Divertline id assigned for jackpot");
            }

            var virtualSAPSearch = await Context.MapPhysicalVirtualSAP.FirstOrDefaultAsync(x => x.VirtualSAPZoneId == mapPhysical.VirtualSAPZoneId);
            if (virtualSAPSearch != null)
            {
                throw new InvalidOperationException("The Virtual Zone ID is already registered..");
            }

            if (mapPhysical.DivertLineId == 0)
            {
                mapPhysical.DivertLine = null;

                mapPhysical.DivertLineId = jackpotDivertLine;

                await Context.MapPhysicalVirtualSAP.AddAsync(mapPhysical);

                await Context.SaveChangesAsync();

                return mapPhysical;
            }

            var divertLine = await Context.DivertLines.FindAsync(mapPhysical.DivertLineId);//<--Editado
            if (divertLine == null)
            {
                throw new InvalidOperationException("The DivertLine ID does not exist in the database..");
            }

            mapPhysical.DivertLine = null;

            await Context.MapPhysicalVirtualSAP.AddAsync(mapPhysical);

            divertLine.MapPhysicalVirtualSAPs.Add(mapPhysical);

            await Context.SaveChangesAsync();

            return mapPhysical;
        }

        public override async Task<MapPhysicalVirtualSAP> GetAsync(int id)
        {
            var mapPhysical = await Context.MapPhysicalVirtualSAP.Include(x => x.DivertLine).FirstOrDefaultAsync(x => x.Id == id);
            return mapPhysical;
        }

        public override async Task<MapPhysicalVirtualSAP> UpdateAsync(MapPhysicalVirtualSAP mapPhysical)
        {
            var mapExisting = await Context.MapPhysicalVirtualSAP.FirstOrDefaultAsync(x => x.Id == mapPhysical.Id);
            if (mapExisting == null)
            {
                throw new InvalidOperationException("The Map Physical ID does not exist in the database..");
            }
            var divertLine = await Context.DivertLines.FindAsync(mapPhysical.DivertLine.Id);
            if (divertLine == null)
            {
                throw new InvalidOperationException("The DivertLine ID does not exist in the database..");
            }

            var virtualSAPSearch = await Context.MapPhysicalVirtualSAP.FirstOrDefaultAsync(x => x.VirtualSAPZoneId == mapPhysical.VirtualSAPZoneId && x.Id != mapPhysical.Id);
            if (virtualSAPSearch != null)
            {
                throw new InvalidOperationException("The Virual Zone ID is already registered..");
            }

            var entity = await Context.MapPhysicalVirtualSAP.FindAsync(mapPhysical.Id);

            if (entity.VirtualSAPZoneId != mapPhysical.VirtualSAPZoneId)
            {
                throw new InvalidOperationException("The DivertLine ID cannot be changed.");
            }
            entity.VirtualSAPZoneId = mapPhysical.VirtualSAPZoneId;
            entity.DivertLine = divertLine;

            await Context.SaveChangesAsync();

            return entity;
        }

        public override IQueryable<MapPhysicalVirtualSAP> GetAll()
        {
            var jackpotDivertLine = (from j in Context.JackpotLines where j.JackpotLineValue == true select j.DivertLineId).FirstOrDefault();
            if (jackpotDivertLine == null)
            {
                throw new InvalidOperationException("There is no Divertline id assigned for jackpot");
            }

            var existingVirtualSAPIds = Context.MapPhysicalVirtualSAP.Select(x => x.VirtualSAPZoneId).ToList();

            int recordsToAdd = 99 - existingVirtualSAPIds.Count;

            if (recordsToAdd > 0)
            {
                for (int i = 1; i <= recordsToAdd; i++)
                {
                    int newVirtualSAPZoneId = 1;
                    while (existingVirtualSAPIds.Contains(newVirtualSAPZoneId))
                    {
                        newVirtualSAPZoneId++;
                    }

                    MapPhysicalVirtualSAP mapPhysical = new MapPhysicalVirtualSAP
                    {
                        DivertLineId = jackpotDivertLine,
                        VirtualSAPZoneId = newVirtualSAPZoneId
                    };

                    mapPhysical.DivertLine = null;

                    Context.MapPhysicalVirtualSAP.Add(mapPhysical);
                    existingVirtualSAPIds.Add(newVirtualSAPZoneId);
                }
            }

            Context.SaveChanges();

            var updatedMapPhysical = from m in Context.MapPhysicalVirtualSAP
                                     join d in Context.DivertLines on m.DivertLineId equals d.Id
                                     where m.DivertLine.DivertLineValue == d.DivertLineValue
                                     select new MapPhysicalVirtualSAP
                                     {
                                         Id = m.Id,
                                         DivertLineId = d.DivertLineValue,
                                         VirtualSAPZoneId = m.VirtualSAPZoneId
                                     };
            return updatedMapPhysical;
        }

        public async Task<List<MapPhysicalGetAllDto>> GetAllMaps()
        {
            var jackpotDivertLine = (from j in Context.JackpotLines where j.JackpotLineValue == true select j.DivertLineId).FirstOrDefault();
            if (jackpotDivertLine == null)
            {
                throw new InvalidOperationException("There is no Divertline id assigned for jackpot");
            }

            var existingVirtualSAPIds = Context.MapPhysicalVirtualSAP.Select(x => x.VirtualSAPZoneId).ToList();

            int recordsToAdd = 99 - existingVirtualSAPIds.Count;

            if (recordsToAdd > 0)
            {
                for (int i = 1; i <= recordsToAdd; i++)
                {
                    int newVirtualSAPZoneId = 1;
                    while (existingVirtualSAPIds.Contains(newVirtualSAPZoneId))
                    {
                        newVirtualSAPZoneId++;
                    }

                    MapPhysicalVirtualSAP mapPhysical = new MapPhysicalVirtualSAP
                    {
                        DivertLineId = jackpotDivertLine,
                        VirtualSAPZoneId = newVirtualSAPZoneId
                    };

                    mapPhysical.DivertLine = null;

                    Context.MapPhysicalVirtualSAP.Add(mapPhysical);
                    existingVirtualSAPIds.Add(newVirtualSAPZoneId);
                }
            }

            Context.SaveChanges();

            var updatedMapPhysical = from m in Context.MapPhysicalVirtualSAP
                                     join d in Context.DivertLines on m.DivertLineId equals d.Id
                                     where m.DivertLine.DivertLineValue == d.DivertLineValue
                                     orderby m.VirtualSAPZoneId ascending
                                     select new MapPhysicalGetAllDto
                                     {
                                         Id = m.Id,
                                         DivertLineId = m.DivertLineId,
                                         DivertLineValue = d.DivertLineValue,
                                         VirtualSAPZoneId = m.VirtualSAPZoneId
                                     };
            var result = await updatedMapPhysical.ToListAsync();
            return result;
        }

        public IQueryable<MapPhysicalGetAllDto> GetByDivertLineId(int divertLineId)
        {
            var result = from f in Context.MapPhysicalVirtualSAP
                         join d in Context.DivertLines on f.DivertLineId equals d.Id
                         where f.DivertLine.DivertLineValue == divertLineId
                         select new MapPhysicalGetAllDto
                         {
                             Id = f.Id,
                             DivertLineId = f.DivertLineId,
                             DivertLineValue = d.DivertLineValue,
                             VirtualSAPZoneId = f.VirtualSAPZoneId
                         };
            return result;
        }

        public int GetDiverJack()
        {
            var jackpotDivertLine = (from j in Context.JackpotLines where j.JackpotLineValue == true select j.DivertLineId).FirstOrDefault();
            if (jackpotDivertLine == null)
            {
                throw new InvalidOperationException("There is no Divertlien id assigned");
            }
            return jackpotDivertLine;
        }

        public async Task<int> UpdateValuesDiverlineJk(int oldJackpot)
        {

            var jackpotDivertLine = (from j in Context.JackpotLines where j.JackpotLineValue == true select j.DivertLineId).FirstOrDefault();
            if (jackpotDivertLine == null)
            {
                throw new InvalidOperationException("There is no Divertline id assigned for jackpot");
            }

            // var existingVirtualSAPIds = Context.MapPhysicalVirtualSAP.Select(x => x.VirtualSAPZoneId).ToList();
            // int recordsToAdd = 99 - existingVirtualSAPIds.Count;

            // if (recordsToAdd > 0)
            // {
            //     for (int i = 1; i <= recordsToAdd; i++)
            //     {
            //         int newVirtualSAPZoneId = 1;
            //         while (existingVirtualSAPIds.Contains(newVirtualSAPZoneId))
            //         {
            //             newVirtualSAPZoneId++;
            //         }

            //         MapPhysicalVirtualSAP mapPhysical = new MapPhysicalVirtualSAP
            //         {
            //             DivertLineId = jackpotDivertLine,
            //             VirtualSAPZoneId = newVirtualSAPZoneId
            //         };

            //         mapPhysical.DivertLine = null;

            //         Context.MapPhysicalVirtualSAP.Add(mapPhysical);
            //         existingVirtualSAPIds.Add(newVirtualSAPZoneId);
            //     }
            // }

            var mapPhysicalToUpdate = Context.MapPhysicalVirtualSAP.Where(x => x.DivertLineId == oldJackpot).ToList();
            mapPhysicalToUpdate.ForEach(x => x.DivertLineId = jackpotDivertLine);

            Context.SaveChanges();

            // return recordsToAdd;
            return mapPhysicalToUpdate.Count;
        }

        public async Task<int> GetJackpotAssignment()
        {
            var jackpotExist = await Context.MapPhysicalVirtualSAP.Where(mp => mp.VirtualSAPZoneId == 999).Select(mp => mp).FirstOrDefaultAsync();

            if (jackpotExist == null)
            {
                var jackpotDivertId = await Context.JackpotLines.FirstOrDefaultAsync() ?? null;

                var RegisterJackpotInMapPhysical = new MapPhysicalVirtualSAP
                {
                    DivertLineId = jackpotDivertId.DivertLineId,
                    VirtualSAPZoneId = 999
                };

                await Context.MapPhysicalVirtualSAP.AddAsync(RegisterJackpotInMapPhysical);
                await Context.SaveChangesAsync();
            }

            return 999;
        }
    }
}
