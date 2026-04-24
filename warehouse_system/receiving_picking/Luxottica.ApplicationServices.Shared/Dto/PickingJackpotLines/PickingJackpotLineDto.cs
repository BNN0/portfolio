using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.Shared.Dto.PickingJackpotLines
{
    public class PickingJackpotLineDto
    {
        ////Required
        //public int Id { get; set; }

        //Required
        public bool PickingJackpotLineValue { get; set; }

        //Required
        public int DivertLineId { get; set; }
    }
}
