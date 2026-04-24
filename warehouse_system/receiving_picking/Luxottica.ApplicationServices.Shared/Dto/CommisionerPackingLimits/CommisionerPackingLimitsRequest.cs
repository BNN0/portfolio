using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.Shared.Dto.CommisionerPackingLimits
{
    public class CommisionerPackingLimitsRequest
    {
        [Required]
        public int SetLimitSuresort_1 { get; set; }
        [Required]
        public int SetLimitSuresort_2 { get; set; }
        [Required]
        public int SetLimitPutWall_1 { get; set; }
        [Required]
        public int SetLimitPutWall_2 { get; set; }
    }
}
