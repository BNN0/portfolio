using LuxotticaSorting.Core.Container;
using LuxotticaSorting.Core.DivertLanes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.Core.ContainerTypes
{
    public class ContainerType
    {
        [Key]
        public int Id { get; set; }
        public string ContainerTypes { get; set; }
        public ICollection<Container.ContainerTable> Containers { get; set; }
    }
}
