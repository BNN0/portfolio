using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Shared.DTO.RecirculationLimits
{
    public class RecirculationLimitDto
    {
        [Key]
        public int Id { get; set; }
        public int CountLimit { get; set; }
    }
}
