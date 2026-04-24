using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodeLogisticAgent
{
    public class CarrierCodeLogisticAgentGetAllDto
    {
        public int Id { get; set; }
        public int CarrierCodeId { get; set; }
        public string CarrierCodes { get; set; }
        public int LogisticAgentId { get; set; }
        public string LogisticAgents { get; set; }
    }
}
