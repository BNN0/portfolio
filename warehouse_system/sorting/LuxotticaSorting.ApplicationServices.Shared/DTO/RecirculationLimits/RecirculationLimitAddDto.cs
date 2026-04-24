using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Shared.DTO.RecirculationLimits
{
    public class RecirculationLimitAddDto
    {
        [Required]
        [Range(0, int.MaxValue)]
        public int CountLimit { get; set; }
    }
}
