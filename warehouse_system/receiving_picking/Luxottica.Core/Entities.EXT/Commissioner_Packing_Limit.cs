using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.Core.Entities.EXT
{
    public class Commissioner_Packing_Limits
    {
        [Key]
        public int Id { get; set; }

        [Column("Put_Station_Nr")]
        public int PutStationNr { get; set; }

        public int Limit { get; set; }
    }
}
