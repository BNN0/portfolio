using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Shared.Dto.LogisticAgents
{
    public class LogisticAgentAddDto
    {
        [Required]
        public string LogisticAgents { get; set; }
    }
}
