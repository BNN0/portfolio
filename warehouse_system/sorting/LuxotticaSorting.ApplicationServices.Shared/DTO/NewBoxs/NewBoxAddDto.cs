using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Shared.DTO.NewBoxs
{
    public class NewBoxAddDto
    {
        public string BoxId { get; set; }
        public string CarrierCode { get; set; }
        public string LogisticAgent { get; set; }
        public string BoxType { get; set; }
    }
}
