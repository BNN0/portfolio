
using Luxottica.ApplicationServices.Scanlogs;
using Luxottica.ApplicationServices.Shared.Dto.Scanlogs;
using Luxottica.ApplicationServices.Shared.Dto.ToteInformation;
using Luxottica.Core.Entities.Cameras;
using Luxottica.Core.Entities.ToteInformations;
using Luxottica.DataAccess.Repositories;
using Luxottica.DataAccess.Repositories.ToteInformation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Luxottica.ApplicationServices.ToteInformations
{
    public class ToteInformationAppService : IToteInformationAppService
    {
        private readonly IRepository<int, ToteInformationE> _repository;
        private readonly IRepository<int, Camera> _repositoryCamera;
        private readonly ToteInformationRepository _toteInfoRepository;
        private readonly IScanlogsAppService _scanlogsAppService;
        private readonly ILogger<ToteInformationAppService> _logger;

        public ToteInformationAppService(IRepository<int, ToteInformationE> repository, ToteInformationRepository toteInfoRepository, IRepository<int, Camera> repositoryCamera, ILogger<ToteInformationAppService> logger, IScanlogsAppService scanlogsAppService)
        {
            _repository = repository;
            _toteInfoRepository = toteInfoRepository;
            _repositoryCamera = repositoryCamera;
            _logger = logger;
            _scanlogsAppService = scanlogsAppService;
        }
        public async Task<int> AddToteInformationAsync(ToteInformationE toteInformation)
        {
            try
            {
                await _repository.AddAsync(toteInformation);
                return toteInformation.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT ToteInformation IN ToteInformationAppService, Message {ex.Message}");
                throw new Exception($"AddToteInformationAsync unsuccessful. Error: {ex.Message}");
            }

        }

        public async Task<int> CheckTote(string LPN, int trackingId/*, int zoneId*/)
        {
            try
            {
                var result = await _toteInfoRepository.CheckTote(LPN, trackingId /*, zoneId*/);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR CheckTote IN ToteInformationAppService, Message {ex.Message}");
                throw new Exception($"CheckTote process unsuccessful: {ex.Message}");
            }
        }

        public async Task<int> DivertTote(string LPN, string CamId, int trakingId)
        {
            try
            {
                var result = await _toteInfoRepository.DivertTote(LPN, CamId, trakingId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DivertTote Method IN ToteInformationAppService, Message {ex.Message}");
                throw new Exception($"Error internal:  {ex.Message}");
            }
        }

        public async Task DeleteToteInformationAsync(int toteInformationId)
        {
            try
            {
                await _repository.DeleteAsync(toteInformationId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR Delete ToteInformation Where Id = {toteInformationId} IN ToteInformationAppService, Message {ex.Message}");
                throw new Exception($"DeleteToteInformationAsync unsuccessful. Error: {ex.Message}");
            }

        }

        public async Task EditToteInformationAsync(ToteInformationE toteInformation)
        {
            try
            {
                await _repository.UpdateAsync(toteInformation);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR UPDATE ToteInformation Where Id = {toteInformation.Id} IN ToteInformationAppService, Message {ex.Message}");
                throw new Exception($"EditToteInformationAsync unsuccessful. Error: {ex.Message}");
            }

        }

        public async Task<ToteInformationE> GetToteInformationByIdAsync(int toteInformationId)
        {
            try
            {
                var result = await _repository.GetAsync(toteInformationId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT ToteInformation Where Id = {toteInformationId} IN ToteInformationAppService, Message {ex.Message}");
                throw new Exception($"GetToteInformationByIdAsync unsuccessful. Error: {ex.Message}");
            }

        }

        public async Task<List<ToteInformationE>> GetTotesAsync()
        {
            try
            {
                var result = await _repository.GetAll().ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT ToteInformation IN ToteInformationAppService, Message {ex.Message}");
                throw new Exception($"GetTotesAsync unsuccessful. Error: {ex.Message}");
            }

        }

        public async Task<DivertCodeBundleModel> ValidateTote(string toteLPN, string camId, int trackingId)
        {
            try
            {
                if (await ValidateCam(camId))
                {
                    if ((toteLPN.ToUpper().StartsWith("H") || toteLPN.ToUpper().StartsWith("K") || toteLPN.ToUpper().StartsWith("N")))
                    {
                        var result = await _repository.GetAll().Where(x => x.ToteLPN == toteLPN && x.DivertStatus == null).Select(x => x).FirstOrDefaultAsync();
                        if (result != null)
                        {
                            result.TrackingId = trackingId;
                            await _repository.UpdateAsync(result);
                            #region Scanlog
                            await _scanlogsAppService.AddScanlogAsync(new ScanlogsAddDto
                            {
                                ToteLPN = toteLPN,
                                TrackingId = trackingId,
                                Timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                                CamId = camId,
                                DivertCode = 1,
                                Info = "Tote is Receiving, it is not possible to pass"
                            });
                            #endregion
                            return new DivertCodeBundleModel
                            {
                                divert_code = 1,
                                tracking_id = result.TrackingId
                            };
                        }
                    }
                    if ((toteLPN.ToUpper().StartsWith("T")))
                    {
                        var resulTote = await _repository.GetAll().Where(x => x.ToteLPN == toteLPN && x.DivertStatus == null).Select(x => x).FirstOrDefaultAsync();
                        if (resulTote != null)
                        {
                            resulTote.TrackingId = trackingId;
                            await _repository.UpdateAsync(resulTote);

                            var toteCode = _toteInfoRepository.CheckLimit(toteLPN, camId, trackingId);
                            var result = toteCode.Result;
                            if (result == 99)
                            {
                                var result1 = await _repository.GetAll().Where(x => x.ToteLPN == toteLPN && x.DivertStatus == null).Select(x => x).FirstOrDefaultAsync();
                                return new DivertCodeBundleModel
                                {
                                    divert_code = 99,
                                    tracking_id = result1.TrackingId
                                };
                            }
                        }
                    }
                }
                return new DivertCodeBundleModel
                {
                    divert_code = 1,
                    tracking_id = trackingId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR BorderPicking IN ToteInformationAppService, Message {ex.Message}");
                throw new Exception($"An error has ocurred in the process: {ex.Message}");
            }
        }

        public async Task<bool> ValidateCam(string camId)
        {
            var result = await _repositoryCamera.GetAll().Where(c => c.CamId == camId).Select(c => c.Id).FirstOrDefaultAsync();
            var cam = await _repositoryCamera.GetAsync(result);
            try
            {
                if (cam == null)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error has ocurred in the process: {ex.Message}");
            }
        }

        public async Task<int> DivertToteSingle(string LPN, string CamId, int trakingId)
        {
            try
            {
                var result = await _toteInfoRepository.DivertToteSingle(LPN, CamId, trakingId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DivertToteSingle IN ToteInformationAppService, Message {ex.Message}");
                throw new Exception($"Error internal:  {ex.Message}");
            }
        }

        public async Task<int> DivertToteMulti(string LPN, string CamId, int trakingId)
        {
            try
            {
                var result = await _toteInfoRepository.DivertToteMulti(LPN, CamId, trakingId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DivertToteMulti IN ToteInformationAppService, Message {ex.Message}");
                throw new Exception($"Error internal:  {ex.Message}");
            }
        }

        public async Task<int> DivertConfirm(string timestamp, int trackingId, string camId)
        {
            try
            {
                var resp = await _toteInfoRepository.DivertConfirm(timestamp, trackingId ,camId);
                return resp;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DivertToteConfirm IN ToteInformationAppService, Message {ex.Message}");
                throw new Exception($"Divert Confirm process unsuccessful");
            }
        }
        
        public async Task<int> DivertConfirmCam14(string timestamp, int trackingId, string camId)
        {
            try
            {
                var resp = await _toteInfoRepository.DivertConfirmCam14(timestamp, trackingId, camId);
                return resp;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DivertToteConfirmCam14 IN ToteInformationAppService, Message {ex.Message}");
                throw new Exception($"Divert Confirm process unsuccessful");
            }
        }
        
        public async Task<int> DivertConfirmCam15(string timestamp, int trackingId, string camId)
        {
            try
            {
                var resp = await _toteInfoRepository.DivertConfirmCam15(timestamp, trackingId, camId);
                return resp;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DivertToteConfirmCam15 IN ToteInformationAppService, Message {ex.Message}");
                throw new Exception($"Divert Confirm process unsuccessful");
            }
        }

        public async Task<int> DivertConfirmCam16(string timestamp, int trackingId, string camId)
        {
            try
            {
                var resp = await _toteInfoRepository.DivertConfirmCam16(timestamp, trackingId, camId);
                return resp;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DivertToteConfirmCam16 IN ToteInformationAppService, Message {ex.Message}");
                throw new Exception($"Divert Confirm process unsuccessful");
            }
        }

        public async Task<int> DivertConfirmCam17(string timestamp, int trackingId, string camId)
        {
            try
            {
                var resp = await _toteInfoRepository.DivertConfirmCam17(timestamp, trackingId, camId);
                return resp;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DivertToteConfirmCam17 IN ToteInformationAppService, Message {ex.Message}");
                throw new Exception($"Divert Confirm process unsuccessful");
            }
        }

        public async Task<bool> IsJackpotLine(string camId)
        {
            var result = await _toteInfoRepository.IsJackpotLine(camId);
            return result;
        }

        public async Task<bool> IsPickingJackpotLine(string camId)
        {
            var result = await _toteInfoRepository.IsPickingJackpotLine(camId);
            return result;
        }

        public async Task<int> DivertMultiToteCam14(string LPN, string CamId, int trakingId)
        {
            try
            {
                var result = await _toteInfoRepository.DivertMultiToteCam14(LPN, CamId, trakingId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DivertToteMultiToteCam14 IN ToteInformationAppService, Message {ex.Message}");
                throw new Exception($"Error internal:  {ex.Message}");
            }
        }

        public async Task<int> DivertMultiToteCam15(string LPN, string CamId, int trakingId)
        {
            try
            {
                var result = await _toteInfoRepository.DivertMultiToteCam15(LPN, CamId, trakingId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DivertToteMultiToteCam15 IN ToteInformationAppService, Message {ex.Message}");
                throw new Exception($"Error internal:  {ex.Message}");
            }
        }

        public async Task<int> DivertMultiToteCam16(string LPN, string CamId, int trakingId)
        {
            try
            {
                var result = await _toteInfoRepository.DivertMultiToteCam16(LPN, CamId, trakingId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DivertToteMultiToteCam16 IN ToteInformationAppService, Message {ex.Message}");
                throw new Exception($"Error internal:  {ex.Message}");
            }
        }

        public async Task<int> DivertMultiToteCam17(string LPN, string CamId, int trakingId)
        {
            try
            {
                var result = await _toteInfoRepository.DivertMultiToteCam17(LPN, CamId, trakingId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DivertToteMultiToteCam17 IN ToteInformationAppService, Message {ex.Message}");
                throw new Exception($"Error internal:  {ex.Message}");
            }
        }

        public async Task<int> DivertTotesInCam11(string LPN, string CamId, int trakingId)
        {
            try
            {
                var result = await _toteInfoRepository.DiverTotesInCam11(LPN, CamId, trakingId);
                return result;
            }
            catch(Exception ex)
            {
                _logger.LogError($"ERROR DivertToteCam11 IN ToteInformationAppService, Message {ex.Message}");
                throw new Exception($"Error internal:  {ex.Message}");
            }
        }
    }
}