using Luxottica.ApplicationServices.Shared.Dto.MapDivert;
using Luxottica.ApplicationServices.Shared.Dto.MapPhysicVirtualSAP;
using Luxottica.Core.Entities.JackpotLines;
using Luxottica.Core.Entities.PhysicalMaps;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.DataAccess.Repositories.MapDivert
{
    public class MapDivertRepository : Repository<int, MapPhysicalVirtualSAP>
    {
        public MapDivertRepository(V10Context context) : base(context)
        {
        }

        public async Task<bool> AssignVirtualZone(int id, VectorModel vector)
        {
            var divertLine = await Context.DivertLines.FirstOrDefaultAsync(x => x.Id == id);
            if (divertLine == null)
            {
                return false;
            }
            var virtualzones = await Context.MapPhysicalVirtualSAP
                .Where(x => x.DivertLineId == divertLine.Id)
                .ToListAsync();
            if (virtualzones == null)
            {
                return false;
            }
            var jackpotDivertLine = (from j in Context.JackpotLines where j.JackpotLineValue == true select j.DivertLineId).FirstOrDefault();

            if (jackpotDivertLine == null)
            {
                return false;
            }
            if (divertLine.StatusLine == false)
            {
                throw new Exception("The divertline is deactivated.");
            }

            for (int i = 0; i < vector.Values.Count; i++)
            {
                var registroDeseado = Context.MapPhysicalVirtualSAP.FirstOrDefault(item => item.VirtualSAPZoneId == vector.Values[i]);

                if (registroDeseado != null)
                {
                    registroDeseado.DivertLineId = id;
                    registroDeseado.VirtualSAPZoneId = registroDeseado.VirtualSAPZoneId;
                    virtualzones.RemoveAll(item => item.VirtualSAPZoneId == vector.Values[i]);
                }
            }
            foreach (var item in virtualzones)
            {
                var registroDeseado = Context.MapPhysicalVirtualSAP.FirstOrDefault(i => i.Id == item.Id);
                registroDeseado.DivertLineId = jackpotDivertLine;
            }
            await Context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> OnlyDivertValue(int id)
        {
            var divertLine = await Context.DivertLines.FirstOrDefaultAsync(x => x.Id == id);
            if (divertLine == null)
            {
                return false;
            }
            var virtualzones = await Context.MapPhysicalVirtualSAP
                .Where(x => x.DivertLineId == divertLine.Id)
                .ToListAsync();
            if (virtualzones == null)
            {
                return true;
            }
            var jackpotDivertLine = (from j in Context.JackpotLines where j.JackpotLineValue == true select j.DivertLineId).FirstOrDefault();

            if (jackpotDivertLine == null)
            {
                return false;
            }
            foreach (var item in virtualzones)
            {
                var registroDeseado = Context.MapPhysicalVirtualSAP.FirstOrDefault(i => i.Id == item.Id);
                registroDeseado.DivertLineId = jackpotDivertLine;
            }
            await Context.SaveChangesAsync();
            return true;
        }



    }
}

