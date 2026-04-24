using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Shared.DTO.Zebra
{
    public class ZebraConfigurationDTO
    {
        [Key]
        public int Id { get; set; }

        [StringLength(35)]
        public string NamePrinter { get; set; }

        [StringLength(75)]
        public string? HostName { get; set; }

        [StringLength(15)]
        public string Ip { get; set; }

        public int Port { get; set; }

        [StringLength(3)]
        public string PortType { get; set; }
    }
}
