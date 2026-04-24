using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.Shared.DTO.PrintLabel
{
    public class PrintLabelMultiBoxDto
    {
        public string ConfirmationNumber { get; set; }

        //public string ConfirmationOrder { get; set; }

        public List<string> BoxesProperlyDelivered { get; set; }

        public List<string> BoxesNotDeliveredDueToClosure { get; set; }
    }
}
