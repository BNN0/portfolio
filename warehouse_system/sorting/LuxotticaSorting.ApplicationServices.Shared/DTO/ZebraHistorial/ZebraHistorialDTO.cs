using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Shared.DTO.ZebraHistorial
{
    public class ZebraHistorialDTO
    {
        [Key]
        public int Id { get; set; }


        public int DivertLaneId { get; set; }


        public int ContainerId { get; set; }


        public int ZebraConfigurationId { get; set; }

        [StringLength(20)]
        public string Timestamp { get; set; }

        public bool Status { get; set; }
    }
}
