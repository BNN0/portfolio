using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.DataAccess
{
    public class SAPContext : DbContext
    {
        public SAPContext(DbContextOptions<SAPContext> options) : base(options)
        {
        }
    }
}
