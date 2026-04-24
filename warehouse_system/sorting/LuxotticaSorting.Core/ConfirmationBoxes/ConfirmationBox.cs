using LuxotticaSorting.Core.WCSRoutingV10;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.Core.ConfirmationBoxes
{
    public class ConfirmationBox
    {
        [Key]
        public int Id { get; set; }
        public int BoxId { get; set; }

        public string ContainerId { get; set; }
        public int DivertLane {  get; set; }
    }
}
