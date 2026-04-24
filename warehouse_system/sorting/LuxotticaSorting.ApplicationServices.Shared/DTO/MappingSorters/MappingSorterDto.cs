using LuxotticaSorting.Core.BoxTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Shared.DTO.MappingSorters
{
    public class MappingSorterDto
    {
        public int Id { get; set; }
        public int DivertLaneId { get; set; }
        public int LogisticAgentId { get; set; }
        public string? CarrierCodeId { get; set; }
        public int ContainerTypeId { get; set; }
        public string? BoxTypeId { get; set; }
    }
}
