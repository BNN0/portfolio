using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.Shared.Dto.DivertLines
{
    public class DivertLineAddDto
    {
        [Required]
        public int DivertLineValue { get; set; }

        [Required]
        public bool StatusLine { get; set; }
    }
}
