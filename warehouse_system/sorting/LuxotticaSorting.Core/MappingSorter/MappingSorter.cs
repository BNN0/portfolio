using LuxotticaSorting.Core.CarrierCodes;
using LuxotticaSorting.Core.ContainerTypes;
using LuxotticaSorting.Core.DivertLanes;
using LuxotticaSorting.Core.LogisticAgents;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxotticaSorting.Core.MappingSorter
{
    public class MappingSorter
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("DivertLane")]
        public int DivertLaneId { get; set; }

        [ForeignKey("LogisticAgent")]
        public int? LogisticAgentId { get; set; }

        public string? CarrierCodeId { get; set; }

        [ForeignKey("ContainerType")]
        public int? ContainerTypeId { get; set; }

        public string? BoxTypeId { get; set; }

        public DivertLane DivertLane { get; set; }
        public LogisticAgent? LogisticAgent { get; set; }
        public ContainerType? ContainerType { get; set; }
    }
}
