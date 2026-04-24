using Luxottica.Core.Entities.DivertLines;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.Core.Entities.JackpotLines
{
    public class JackpotLine
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public bool JackpotLineValue { get; set; }

        [Required]
        public int DivertLineId { get; set; }

        public DivertLine DivertLine { get; set; }

    }
}
