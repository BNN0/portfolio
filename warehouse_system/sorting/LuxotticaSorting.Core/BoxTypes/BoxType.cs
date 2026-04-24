using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.Core.BoxTypes
{
    public class BoxType
    {
        [Key]
        public int Id {  get; set; }
        
        public string BoxTypes { get; set; }
    }
}
