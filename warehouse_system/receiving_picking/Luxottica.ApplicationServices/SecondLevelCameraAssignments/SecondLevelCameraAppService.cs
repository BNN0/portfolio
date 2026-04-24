using AutoMapper;
using Luxottica.ApplicationServices.JackpotLines;
using Luxottica.ApplicationServices.Shared.Dto.SecondLevelCamera;
using Luxottica.Core.Entities.CameraAssignments.SecondLevel;
using Luxottica.Core.Entities.Cameras;
using Luxottica.DataAccess.Repositories;
using Luxottica.DataAccess.Repositories.SecondLevelCameraAssignments;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.SecondLevelCameraAssignments
{
    public class SecondLevelCameraAppService : ISecondLevelCameraAppService
    {
        private readonly IRepository<int, SecondLevelCamera> _repository;
        private readonly SecondLevelCameraAsignmentRepository _secondLevelCameraAsignmentRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<SecondLevelCameraAppService> _logger;

        public SecondLevelCameraAppService(IRepository<int, SecondLevelCamera> repository, SecondLevelCameraAsignmentRepository secondLevelCameraAsignmentRepository, IMapper mapper, ILogger<SecondLevelCameraAppService> logger)
        {
            _repository = repository;
            _secondLevelCameraAsignmentRepository = secondLevelCameraAsignmentRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<bool> ChangeSecondLevelCamera(int id)
        {
            try
            {
                var value = await _secondLevelCameraAsignmentRepository.ChangeSecondLevelCameraAssignment(id);
                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR UPDATE SecondLevelCamerat in AddCameraAssignmentAsync Service Message {ex.Message}");
                throw new Exception($"Failed to change camera assignment for ID {id}");
            }
        }

        public async Task DeleteSecondLevelCamerasAsync(int id)
        {
            try
            {
                var findId = await _repository.GetAsync(id);

                if(findId != null)
                {
                    await _repository.DeleteAsync(id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DELETE SecondLevelCamera in AddCameraAssignmentAsync Service Message {ex.Message}");
                throw new Exception($"Failed to delete Second Level Camera with ID {id}");
            }
        }

        public async Task<SecondLevelCameraBundleDto> GetCameraAssignmentInfo()
        {
            try
            {
                var result = await _secondLevelCameraAsignmentRepository.GetCameraAssignmentInfo();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR GetCameraAssignmentInfo in SecondLevelCameraAppService Message {ex.Message}");
                throw new Exception($"Failed to retrieve Second Level Camera Information");
            }
        }

        public async Task<SecondLevelCameraGetDto> GetSecondLevelCameraAsync()
        {
            try
            {
                var camera = await _secondLevelCameraAsignmentRepository.GetAsync();
                return _mapper.Map<SecondLevelCameraGetDto>(camera);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR GetSecondLevelCameraAsync in SecondLevelCameraAppService Message {ex.Message}");
                throw new Exception($"Failed to retrieve Second Level Camera ");
            }
        }
    }
}
