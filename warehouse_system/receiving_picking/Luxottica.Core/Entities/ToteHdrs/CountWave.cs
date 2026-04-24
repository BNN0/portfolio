using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.Core.Entities.ToteHdrs
{
    public class CountWave
    {
        [Key]
        public int Id { get; set; }
        public string Wave_Nr { get; set; }
        public int Nr_Of_Totes_In_Wave { get; set; }
        public int Count { get; set; }
    }
}
