using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodes
{
    public class CarrierCodeAddDto
    {
        [Required]
        public string CarrierCodes { get; set; }
    }
}
