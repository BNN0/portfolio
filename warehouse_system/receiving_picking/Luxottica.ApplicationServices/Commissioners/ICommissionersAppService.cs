using Luxottica.Core.Entities.Commissioners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.Commissioners
{
    public interface ICommissionersAppService
    {
        Task<List<Commissioner>> GetComissionnersAsync();
        Task<Commissioner> GetFirstCommissionerAsync();
        Task UpdateCommissionerAsync(Commissioner commissioner);
    }
}
