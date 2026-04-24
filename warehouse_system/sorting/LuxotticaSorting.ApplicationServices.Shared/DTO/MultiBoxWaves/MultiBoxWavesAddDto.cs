using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Shared.DTO.MultiBoxWaves
{
    public class MultiBoxWavesAddDto
    {
            public string ContainerId { get; set; }

            public string ContainerType { get; set; }

            public int DivertLane { get; set; }

            public string ConfirmationNumber { get; set; }

            public int Qty { get; set; }
    }
}
