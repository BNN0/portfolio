using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.Core.Entities.EXT
{
    [Table("Tote_Hdr")]
    public class Tote_Hdr
    {
        public int Id { get; set; }
        [StringLength(35)]
        public string Tote_LPN { get; set; }
        [StringLength(35)]
        public string Wave_Nr { get; set; }
        public int Wave_Priority { get; set; }
        public int Nr_Of_Totes_In_Wave { get; set; }
        public int Tote_Total_Qty { get; set; }
        [StringLength(4)]
        public string? WCS_Destination_Area {  get; set; }
        [StringLength(1)]
        public string? RFID_Relevance { get; set; }
        [StringLength(50)]
        public string Timestamp { get; set; }
        public int? Put_Station_Nr { get; set; }
        public int? Status { get; set; }
        public int? Release {  get; set; }
        public bool? Processed { get; set; }
    }
}
