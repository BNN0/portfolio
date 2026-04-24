using System.ComponentModel.DataAnnotations;

namespace Luxottica.Models.Tote
{
    public class ToteModel
    {
        [Required]
        [StringLength(10)]
        public string toteLpn { get; set; }

        [Required]
        [StringLength(35)]
        
        public string VirtualTote { get; set; }

        [Required]
        public int ZoneDivertId { get; set; }

        [StringLength(2)]
        [MinLength(2)]
        [RegularExpression("^(in|na|null)$")]
        public string? DivertStatus { get; set; }

        [Required]
        public int LineCount { get; set; }

        [Required]
        public int TrackingId { get; set; }

    }
}
