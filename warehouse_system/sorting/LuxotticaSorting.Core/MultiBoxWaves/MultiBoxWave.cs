using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.Core.MultiBoxWaves
{
    public class MultiBoxWave
    {
        [Key]
        public int Id { get; set; }

        [StringLength(20)]
        public string ContainerId { get; set; }

        [StringLength(1)]
        public string ContainerType { get; set; }

        public int DivertLane { get; set; }

        [StringLength(20)]
        public string ConfirmationNumber { get; set; }

        public int Qty { get; set; }

        public int QtyCount { get; set; }

        [StringLength(2)]
        public string? Status { get; set; }
    }
}
