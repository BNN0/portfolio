using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.Core.CarrierCodes
{
    public class CarrierCode
    {
        [Key]
        public int Id {  get; set; }
        public string CarrierCodes { get; set; }

    }
}
