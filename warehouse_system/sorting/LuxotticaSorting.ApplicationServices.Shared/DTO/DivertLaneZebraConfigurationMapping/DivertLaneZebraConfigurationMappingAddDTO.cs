using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Shared.DTO.DivertLaneZebraConfigurationMapping
{
    public class DivertLaneZebraConfigurationMappingAddDTO
    {

        [Required]
        public int DivertLaneId { get; set; }

        [Required]
        public int ZebraConfigurationId { get; set; }
    }
}
