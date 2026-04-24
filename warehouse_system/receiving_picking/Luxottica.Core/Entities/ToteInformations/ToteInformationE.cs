using Luxottica.Core.Entities.DivertLines;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.Core.Entities.ToteInformations
{
    public class ToteInformationE
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string ToteLPN { get; set; }

        [Required]
        [StringLength(17)]
        [MinLength(16)]
        public string Timestamp { get; set; }

        [StringLength(17)]
        [MinLength(16)]
        public string? DivTimestamp { get; set; }

        [StringLength(6)]
        public string? LastCam {  get; set; }

        [Required]
        [StringLength(35)]
        public string VirtualTote { get; set; }

        [Required]
        public int ZoneDivertId { get; set; }

        [StringLength(2)]
        [MinLength(1)]
        public string? DivertStatus { get; set; }

        [Required]
        public int LineCount { get; set; }

        [Required]
        public int TrackingId { get; set; }

    }
}
