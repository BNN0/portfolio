using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Shared.DTO.Containers
{
    public class ContainerToPrint
    {
        [Required]
        public string ContainerId { get; set; }
        [Required]
        public int DivertLanesId { get; set; }
    }
}
