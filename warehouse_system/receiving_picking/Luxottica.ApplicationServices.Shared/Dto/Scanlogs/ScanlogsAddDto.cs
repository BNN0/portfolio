using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.Shared.Dto.Scanlogs
{
    public class ScanlogsAddDto
    {
        public string? ToteLPN { get; set; }

        public int? VirtualZone { get; set; }

        public string? VirtualTote { get; set; }

        public string? Wave { get; set; }

        public int? TotesInWave { get; set; }

        public int? TotalQty { get; set; }

        public string? DestinationArea { get; set; }

        public int? PutStation { get; set; }

        public int? Status { get; set; }

        public int? Release { get; set; }

        public bool? Processed { get; set; }

        public string? StatusV10 { get; set; }

        public int? LapCount { get; set; }

        public int? TrackingId { get; set; }

        public string? Timestamp { get; set; }

        public string? CamId { get; set; }

        public int? DivertCode { get; set; }

        public string? Info { get; set; }
    }
}
