using LuxotticaSorting.Core.ContainerTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.Core.Container
{
    public class ContainerTable
    {
        [Key]
        public int Id { get; set; }

        public string ContainerId { get; set; }

        [ForeignKey("ContainerType")]
        public int ContainerTypeId { get; set; }
        
        public bool? Status { get; set; }

        public ContainerType ContainerType { get; set; }
    }
}
