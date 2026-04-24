using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.Core.WCSRoutingV10
{
    public class WCSRoutingV10
    {
        [Key]
        public int Id { get; set; }

        [StringLength(20)]
        public string? BoxId { get; set; }
        [StringLength(18)]
        public string? BoxType { get; set; }
        [StringLength(10)]
        public string? CarrierCode { get; set; }
        [StringLength(4)]
        public string? LogisticAgent { get; set; }
        [StringLength(20)]
        public string? ConfirmationNumber { get; set; }
        [StringLength(20)]
        public string ContainerId { get; set; }
        [StringLength(1)]
        public string ContainerType { get; set; }
        public int Qty { get; set; }
        public int DivertLane { get; set; }
        [StringLength(20)]
        public string CurrentTs { get; set; }
        [StringLength(2)]
        public string Status { get; set; }
        [StringLength(4)]
        public string SAPSystem { get; set; }
        public string? DivertTs { get; set; }
        public int? TrackingId { get; set; }
        public int Count { get; set; }

        public static implicit operator WCSRoutingV10(int v)
        {
            throw new NotImplementedException();
        }
    }
}
