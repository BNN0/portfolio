using System.ComponentModel.DataAnnotations;

namespace Luxottica.Models.NewTote
{
    public class NewToteModel
    {
        [Required]
        [StringLength(10)]
        public string camId { get; set; }
        [Required] 
        public int trakingId { get; set; }
        [Required]
        public string toteLpn { get; set; }
    }
}
