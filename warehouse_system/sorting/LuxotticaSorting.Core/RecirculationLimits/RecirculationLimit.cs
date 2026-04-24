using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.Core.RecirculationLimits
{
    public class RecirculationLimit
    {
        [Key]
        public int Id { get; set; }
        public int CountLimit { get; set; }
    }
}
