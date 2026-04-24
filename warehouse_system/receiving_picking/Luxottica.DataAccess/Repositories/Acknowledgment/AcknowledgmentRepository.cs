using Luxottica.ApplicationServices.Shared.Dto.Acknowledgment;
using Luxottica.Core.Entities.Acknowledgments;
using Luxottica.Core.Entities.DivertLines;
using Luxottica.Core.Entities.ToteHdrs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.DataAccess.Repositories.Acknowledgment
{
    public class AcknowledgmentRepository : Repository<int, Core.Entities.Acknowledgments.Acknowledgment>
    {
        public AcknowledgmentRepository(V10Context context) : base(context)
        {
        }
        public async Task<Core.Entities.Acknowledgments.Acknowledgment> AddAsync(AcknowledgmentAddDTO acknowledgmentAddDTO)
        {
            var acknowledgmentExist = Context.Acknowledgments.FirstOrDefault(a => a.ToteLpn == acknowledgmentAddDTO.ToteLpn);
            if (acknowledgmentExist != null)
            {
                throw new Exception("This TOTELPN already exist");
            }


            var acknowledgment = new Core.Entities.Acknowledgments.Acknowledgment
            {
                ToteLpn = acknowledgmentAddDTO.ToteLpn,
                WaveNr = acknowledgmentAddDTO.WaveNr,
                Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                Status = acknowledgmentAddDTO.Status,
            };
            await Context.Acknowledgments.AddAsync(acknowledgment);
            await Context.SaveChangesAsync();
            return acknowledgment;
        }

        public async Task<Core.Entities.Acknowledgments.Acknowledgment> UpdateAsync(int id, AcknowledgmentAddDTO acknowledgmentAddDTO)
        {

            var entity = await Context.Acknowledgments.FindAsync(id);
            if(entity == null)
            {
                throw new Exception("This acknowledgment does not exist");
            }
            entity.ToteLpn = acknowledgmentAddDTO.ToteLpn;
            entity.WaveNr = acknowledgmentAddDTO.WaveNr;
            entity.Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            entity.Status = acknowledgmentAddDTO.Status;
            await Context.SaveChangesAsync();
            return entity;
        }
    }
}
