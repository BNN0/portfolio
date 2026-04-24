using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.Core.Entities.Acknowledgments
{
    public class Acknowledgment
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(10)]
        public string ToteLpn { get; set; }
        [Required]
        [StringLength(35)]
        public string WaveNr { get; set; }
        [Required]
        [StringLength(17)]
        public string Timestamp { get; set; }
        [Required]
        [StringLength(2)]
        public string Status { get; set; }
    }
}
