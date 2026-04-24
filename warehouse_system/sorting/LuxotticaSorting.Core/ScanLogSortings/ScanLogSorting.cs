using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.Core.ScanLogSortings
{
    public class ScanLogSorting
    {
            public int Id { get; set; }
            public string? BoxId { get; set; }
            public string? BoxType { get; set; }
            public string? CarrierCode { get; set; }
            public string? LogisticAgent { get; set; }
            public string? ContainerId { get; set; }
            public string? ContainerType { get; set; }
            public int? DivertLane { get; set; }
            public string? Timestamp { get; set; }
            public string ? ConfirmationNumber { get; set;}
            public int? Count { get; set; }
            public int? TrackingId { get; set; }
            public int? Qty { get; set; }
            public string? Status { get; set; }
    }
}
