using LuxotticaSorting.Core.BoxTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Shared.DTO.MappingSorters
{
    public class MappingSorterAddDto
    {
        public int LogisticAgentId { get; set; }
        public string? CarrierCodeId { get; set; }
        public string? BoxTypeId { get; set; }
    }
}
