using Luxottica.Core.Entities.EXT;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.DataAccess
{
    public class E1ExtContext : DbContext
    {
        public DbSet<Tote_Hdr> ToteHdrs { get; set; }
        public DbSet<Commissioner_Packing_Limits> Commissioner_Packing_Limits { get; set; }
        public DbSet<SharedTable> SharedTable { get; set; }
        public E1ExtContext(DbContextOptions<E1ExtContext> options) : base(options)
        {
            
        }
    }
}
