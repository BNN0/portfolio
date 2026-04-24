using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.Core.Entities.Scanlogs
{
    public class ScanlogsReceivingPicking
    {
        [Key]
        public int Id { get; set; }

        [StringLength(10)]
        public string? ToteLPN { get; set; }

        public int? VirtualZone { get; set; }

        [StringLength(35)]
        public string? VirtualTote { get; set; }

        [StringLength(35)]
        public string? Wave { get; set; }

        public int? TotesInWave { get; set; }

        public int? TotalQty { get; set; }

        [StringLength(4)]
        public string? DestinationArea { get; set; }

        public int? PutStation { get; set; }

        public int? Status { get; set; }

        public int? Release { get; set; }

        public bool? Processed { get; set; }

        [StringLength(2)]
        public string? StatusV10 { get; set; }

        public int? LapCount { get; set; }

        public int? TrackingId { get; set; }

        [StringLength(17)]
        public string? Timestamp { get; set; }

        [StringLength(10)]
        public string? CamId { get; set; }

        public int? DivertCode { get; set; }

        [StringLength(100)]
        [MinLength(50)]
        public string? Info { get; set; }
    }
}
