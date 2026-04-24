using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.Shared.Dto.JackpotLines
{
    public class JackpotLineDto
    {
        //Required
        public int Id { get; set; }

        //Required
        public bool JackpotLineValue { get; set; }

        //Required
        public int DivertLineId { get; set; }
    }
}
