using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.Core.Entities.EXT
{
    [Table("SharedTable")]
    public class SharedTable
    {
        public int Id { get; set; }

        [StringLength(35)]
        public string Wave_Nr { get; set; }

        public DateTime? ReleasedAt { get; set; }
    }
}
