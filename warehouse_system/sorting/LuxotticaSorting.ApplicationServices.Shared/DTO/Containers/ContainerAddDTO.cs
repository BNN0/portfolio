using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Shared.DTO.Containers
{
    public class ContainerAddDTO
    {
        public string ContainerId { get; set; }

        [ForeignKey("ContainerType")]
        public int ContainerTypeId { get; set; }
    }
}
