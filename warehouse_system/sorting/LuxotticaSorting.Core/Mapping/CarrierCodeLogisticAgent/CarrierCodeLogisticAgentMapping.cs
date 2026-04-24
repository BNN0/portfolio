using LuxotticaSorting.Core.CarrierCodes;
using LuxotticaSorting.Core.LogisticAgents;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.Core.Mapping.CarrierCodeLogisticAgent
{
    public class CarrierCodeLogisticAgentMapping
    {
        [Key]
        public int Id { get; set; }

        public int CarrierCodeId { get; set; }
        public int LogisticAgentId { get; set; }

    }
}
