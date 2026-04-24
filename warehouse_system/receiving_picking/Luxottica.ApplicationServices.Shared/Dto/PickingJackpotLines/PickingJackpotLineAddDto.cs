using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.Shared.Dto.PickingJackpotLines
{
    public class PickingJackpotLineAddDto
    {
        //[Required]
        //public int Id { get; set; }
        //[Required(ErrorMessage = "PickingJackpotLineValue is required")]
        //public bool PickingJackpotLineValue { get; set; }

        [Required(ErrorMessage = "Enter a valid number.")]
        [RegularExpression(@"^[1-9][0-9]?$|^99$", ErrorMessage = "The value must be between 1 and 99.")]
        public int DivertLineId { get; set; }
    }
}
