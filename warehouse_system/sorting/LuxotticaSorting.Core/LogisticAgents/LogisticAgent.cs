using LuxotticaSorting.Core.DivertLanes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.Core.LogisticAgents
{
    public class LogisticAgent
    {
        [Key]
        public int Id { get; set; } 

        public string LogisticAgents { get; set; }

    }
}
