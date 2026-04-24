using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Shared.DTO.BoxTypes
{
    public class BoxTypesAddDTO
    {
        [Required]
        public string BoxTypes { get; set; }
    }
}
