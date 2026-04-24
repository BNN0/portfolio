using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.DataAccess
{
    public class SyncContext : DbContext
    {
        public SyncContext(DbContextOptions<SyncContext> options) : base(options)
        {
        }
    }
}
