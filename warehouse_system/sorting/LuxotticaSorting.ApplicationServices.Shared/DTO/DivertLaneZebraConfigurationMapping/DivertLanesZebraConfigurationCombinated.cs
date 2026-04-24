using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Shared.DTO.DivertLaneZebraConfigurationMapping
{
    public class DivertLanesZebraConfigurationCombinated
    {
        [Key]
        public int Id { get; set; }
        public int DivertLaneId { get; set; }
        public int DivertLaneValue { get; set; }
        public int ZebraConfigurationId { get; set; }
        public string ZebraConfigurationName { get; set; }
    }
}
