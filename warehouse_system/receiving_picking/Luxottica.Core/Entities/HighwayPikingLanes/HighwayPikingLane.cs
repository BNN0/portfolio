using Luxottica.Core.Entities.Commissioners;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.Core.Entities.HighwayPikingLanes
{
    public class HighwayPikingLane
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MultiTotes { get; set; }

        [Required]
        public int MaxTotesLPTUMachine1 { get; set; }

        [Required]
        public int MaxTotesSPTAMachine1 { get; set; }

        [Required]
        public int MaxTotesSPTAMachine2 { get; set; }

        [Required]
        public int CountMultiTotes { get; set; }

        [Required]
        public int CountMaxTotesLPTUMachine1 { get; set; }

        [Required]
        public int CountMaxTotesSPTAMachine1 { get; set; }

        [Required]
        public int CountMaxTotesSPTAMachine2 { get; set; }

        [Required]
        public int CommissionerId { get; set; }
        public Commissioner Commissioner { get; set; }
    }
}
