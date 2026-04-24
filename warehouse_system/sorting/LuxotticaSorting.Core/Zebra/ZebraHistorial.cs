
using LuxotticaSorting.Core.CarrierCodes;
using LuxotticaSorting.Core.Container;
using LuxotticaSorting.Core.DivertLanes;
using LuxotticaSorting.Core.LogisticAgents;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.Core.Zebra
{
    public class ZebraHistorial
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("DivertLane")]
        public int DivertLaneId { get; set; }

        [ForeignKey("Container")]
        public int ContainerId { get; set; }

        [ForeignKey("ZebraConfiguration")]
        public int ZebraConfigurationId { get; set; }

        [StringLength(20)]
        public string Timestamp { get; set; }

        public bool Status { get; set; }

        

        public DivertLane DivertLane { get; set; }
        public ContainerTable Container { get; set; }
        public ZebraConfiguration ZebraConfiguration { get; set; }
    }
}
