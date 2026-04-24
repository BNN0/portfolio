using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Shared.DTO.BoxTypeDivertLaneMapping
{
    public class BoxTypeDivertLaneMenuView
    {
        [Key]
        public int Id { get; set; }
        public int BoxTypeId { get; set; }
        public string BoxTypeValue  { get; set; }
        public int DivertLaneId { get; set; }
        public int DivertLaneValue { get; set; }
    }
}
