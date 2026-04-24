using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Shared.DTO.PrintLabel
{
    public class PrintLabelDTO
    {
        [Required]
        public int ContainerId { get; set; }
        [Required]
        public int DivertLaneId { get; set; }

    }
}
