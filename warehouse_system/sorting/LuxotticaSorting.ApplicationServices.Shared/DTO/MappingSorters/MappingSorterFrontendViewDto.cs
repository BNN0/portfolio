using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Shared.Dto.MappingSorters
{
    public class MappingSorterFrontendViewDto
    {
        public int Id { get; set; }
        public int DivertLaneId { get; set; }
        public int DivertLanes { get; set; }
        public int? ContainerTypeId { get; set; }
        public string? ContainerTypes { get; set; }
    }
}
