using LuxotticaSorting.Core.BoxTypes;
using LuxotticaSorting.Core.DivertLanes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.Core.Mapping.BoxTypeDivertLane
{
    public class BoxTypeDivertLaneMapping
    {
        [Key]
        public int Id { get; set; }
        public int BoxTypeId { get; set; }
        public int DivertLaneId { get; set; }
    }
}
