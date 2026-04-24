using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.Shared.Dto.HighwayPickingLanes
{
    public class HighwayPickingRequest
    {
        public int LimitHighway { get; set; }
        public int LimitLPTUMachine1 { get; set; }
        public int LimitSPTAMachine1 { get; set; }
        public int LimitSPTAMachine2 { get; set; }
    }
}
