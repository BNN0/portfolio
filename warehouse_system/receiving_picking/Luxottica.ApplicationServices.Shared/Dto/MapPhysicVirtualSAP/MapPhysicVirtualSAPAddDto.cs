using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.Shared.Dto.MapPhysicVirtualSAP
{
    public class MapPhysicVirtualSAPAddDto
    {
        [Required(ErrorMessage = "Enter a valid number.")]
        //[RegularExpression(@"^[0-9][0-9]?$|^99$", ErrorMessage = "The value must be between 1 and 99.")]
        public int DivertLineId { get; set; }

        [Required]
        [RegularExpression(@"^[1-9][0-9]?$|^99$", ErrorMessage = "The value must be between 1 and 99.")]
        public int VirtualSAPZoneId { get; set; }
    }
}
