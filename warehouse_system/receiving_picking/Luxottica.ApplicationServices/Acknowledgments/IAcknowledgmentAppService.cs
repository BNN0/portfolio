using Luxottica.ApplicationServices.Shared.Dto.Acknowledgment;
using Luxottica.ApplicationServices.Shared.Dto.Camera;
using Luxottica.Core.Entities.Acknowledgments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.Acknowledgments
{
    public interface IAcknowledgmentAppService
    {
        Task<List<Acknowledgment>> GetAcknowledgmentsAsync();

        Task<int> AddAcknowledgmentAsync(AcknowledgmentAddDTO acknowledgmentAddDto);

        Task DeleteAcknowledgmentAsync(int acknowledgmentId);

        Task<Acknowledgment> GetAcknowledgmentByIdAsync(int acknowledgmentId);

        Task EditAcknowledgmentAsync(int id, AcknowledgmentAddDTO acknowledgmentAddDTO);

    }
}
