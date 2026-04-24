using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.Shared.Dto.ToteInformation
{
    public class ZoneRoutingData
    {
        public int Id { get; set; }

        [StringLength(10)]
        public string Tote_ID { get; set; }

        [StringLength(35)]
        public string Virtual_Tote_ID { get; set; }

        public int Zone { get; set; }

        [StringLength(17)]
        public string Insert_ts { get; set; }

        [StringLength(2)]
        public string Status { get; set; }
    }
}
