using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Shared.DTO.MultiBoxWaves
{
    public class MultiBoxWavesGetAllDto
    {
        public int DivertLane { get; set; }
        public string ConfirmationNumber { get; set; }

        public int Qty { get; set; }
        public string Status { get; set; }
        public string StatusFront { get; set; }

        public List<BoxInfoDto>? Boxes { get; set; }
    }

    public class BoxInfoDto
    {
        public string BoxId { get; set; }
        public string Status { get; set; }
    }
}
