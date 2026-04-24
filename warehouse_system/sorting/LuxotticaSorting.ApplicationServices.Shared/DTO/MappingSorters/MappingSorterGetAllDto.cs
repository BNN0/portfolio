using LuxotticaSorting.Core.BoxTypes;
using LuxotticaSorting.Core.CarrierCodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Shared.DTO.MappingSorters
{
    public class MappingSorterGetAllDto
    {
        public int Id { get; set; }
        public int DivertLaneId { get; set; }
        public int DivertLanes { get; set; }
        public bool Status { get; set; }
        public int LogisticAgentId { get; set; }
        public string LogisticAgents { get; set; }
        public int ContainerTypeId { get; set; }
        public string ContainerTypes { get; set; }
        public List<BoxType> BoxTypes { get; set; }

        public List<CarrierCode> CarrierCodes { get; set; }
    }
}
