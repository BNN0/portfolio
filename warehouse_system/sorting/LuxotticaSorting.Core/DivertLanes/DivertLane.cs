using LuxotticaSorting.Core.Container;
using LuxotticaSorting.Core.LogisticAgents;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.Core.DivertLanes
{
    public class DivertLane
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DivertLanes { get; set; }

        public bool Status { get; set; }

        public bool Full {  get; set; }

        [ForeignKey("Container")]
        public int? ContainerId { get; set; }

        public ContainerTable? Container {  get; set; }
    }
}
