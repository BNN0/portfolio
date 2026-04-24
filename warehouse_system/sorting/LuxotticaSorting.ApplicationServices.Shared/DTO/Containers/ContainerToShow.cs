using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Shared.DTO.Containers
{
    public class ContainerToShow
    {
        public int Id { get; set; }
        public string ContainerId { get; set; }
        public int  ContainerTypeId { get; set; }
        public int DivertLaneId { get; set; }
        public int DivertLaneValue { get; set; }
        public bool ? Status { get; set; }    
    }
}
