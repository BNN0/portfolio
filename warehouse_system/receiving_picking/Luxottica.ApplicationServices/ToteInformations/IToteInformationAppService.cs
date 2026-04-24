
using Luxottica.ApplicationServices.Shared.Dto.ToteInformation;
using Luxottica.Core.Entities.ToteInformations;

namespace Luxottica.ApplicationServices.ToteInformations
{
    public interface IToteInformationAppService
    {
        Task<List<ToteInformationE>> GetTotesAsync();

        Task<int> AddToteInformationAsync(ToteInformationE toteInformation);

        Task DeleteToteInformationAsync(int toteInformationId);

        Task<ToteInformationE> GetToteInformationByIdAsync(int toteInformationId);

        Task EditToteInformationAsync(ToteInformationE toteInformation);

        Task<int> DivertConfirm(string timestamp, int trackingId, string camId);
        Task<int> DivertConfirmCam14(string timestamp, int trackingId, string camId);
        Task<int> DivertConfirmCam15(string timestamp, int trackingId, string camId);
        Task<int> DivertConfirmCam16(string timestamp, int trackingId, string camId);
        Task<int> DivertConfirmCam17(string timestamp, int trackingId, string camId);


        Task<int> CheckTote(string LPN, int trackingId);

        Task<int> DivertTote(string LPN, string CamId, int trakingId);

        Task<DivertCodeBundleModel> ValidateTote(string toteInfo, string camId, int trackingId);
        /*
        //Task<bool> ToteLpnExistsInDatabase(string toteLPN);

        //Task<bool> AreAllTotesDiverted(string toteLPN);

        //Task<bool> CheckWaveNrExists(string toteLPN);

        //Task AssignPickingJackpotLane(string toteLPN);
        */
        Task<int> DivertToteSingle(string LPN, string CamId, int trakingId);

        Task<int> DivertToteMulti(string LPN, string CamId, int trakingId);

        Task<bool> IsJackpotLine(string camId);

        Task<bool> IsPickingJackpotLine(string camId);

        Task<int> DivertMultiToteCam14(string LPN, string CamId, int trakingId);

        Task<int> DivertMultiToteCam15(string LPN, string CamId, int trakingId);

        Task<int> DivertMultiToteCam16(string LPN, string CamId, int trakingId);

        Task<int> DivertMultiToteCam17(string LPN, string CamId, int trakingId);

        Task<int> DivertTotesInCam11(string LPN, string CamId, int trakingId);
        //Task newScanlogRecieving(AddNewScanlogRecieving addNewScanlogRecieving);
        //Task NewScanlogs(string toteLpn, string Cam_Id, int command);
        //Task NeweScanLogsPicking(string totelpn, string Cam_Id, int command);
    }
}