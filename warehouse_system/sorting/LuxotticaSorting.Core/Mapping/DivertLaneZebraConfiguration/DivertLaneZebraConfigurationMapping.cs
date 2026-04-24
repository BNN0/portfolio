using LuxotticaSorting.Core.DivertLanes;
using LuxotticaSorting.Core.Zebra;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.Core.Mapping.DivertLaneZebraConfiguration
{
    public class DivertLaneZebraConfigurationMapping
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("DivertLane")]
        public int DivertLaneId { get; set; }

        [ForeignKey("ZebraConfiguration")]
        public int ZebraConfigurationId { get; set; }

        public DivertLane DivertLane { get; set; }
        public ZebraConfiguration ZebraConfiguration { get; set; }
    }
}
