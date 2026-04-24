using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.DataAccess
{
    public class ReceivingWMSContext : DbContext
    {
        public ReceivingWMSContext(DbContextOptions<ReceivingWMSContext> options) : base(options)
        {
        }
    }

}
