using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Shared.DTO.DivertLanes
{
    public class DivertLanesDTO
    {
        public int Id { get; set; }

        [Required]
        public int DivertLanes { get; set; }

        public bool Status { get; set; }

        public bool Full { get; set; }

        public int LogisticAgentId { get; set; }
    }
}
