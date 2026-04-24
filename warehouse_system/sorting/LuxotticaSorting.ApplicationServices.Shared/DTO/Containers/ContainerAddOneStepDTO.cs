using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Shared.DTO.Containers
{
    public class ContainerAddOneStepDTO
    {
        [ForeignKey("ContainerType")]
        public int ContainerTypeId { get; set; }
    }
}
