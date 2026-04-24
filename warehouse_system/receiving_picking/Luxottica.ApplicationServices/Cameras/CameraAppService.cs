using AutoMapper;
using Luxottica.ApplicationServices.Shared.Dto.Camera;
using Luxottica.Core.Entities.Cameras;
using Luxottica.Core.Entities.ToteInformations;
using Luxottica.DataAccess.Repositories;
using Luxottica.DataAccess.Repositories.Camera;
using Luxottica.DataAccess.Repositories.CameraAssignments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.Cameras
{
    public class CameraAppService : ICameraAppService
    {
        private readonly IRepository<int, Camera> _repository;
        private readonly ILogger<CameraAppService> _logger;
        private readonly IMapper _mapper;
        private readonly CameraRepository _cameraRepository;
        public CameraAppService(IRepository<int, Camera> repository, IMapper mapper, CameraRepository cameraRepository, ILogger<CameraAppService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _cameraRepository = cameraRepository;
            _logger = logger;
        }

        public async Task<int> AddCameraAsync(CameraDTO camera)
        {
            try
            {
                var entity = _mapper.Map<CameraDTO, Camera>(camera);
                await _cameraRepository.AddAsync(entity);
                return entity.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR INSERT CAMERA in AddCameraAsync SERVICE {ex.Message}");
                throw new Exception($"AddCameraAsync unsuccessful. Error: {ex.Message}");
            }

        }

        public async Task DeleteCameraAsync(int cameraId)
        {
            try
            {
                await _repository.DeleteAsync(cameraId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DELETE CAMERA WHERE Id= {cameraId} in DeleteCameraAsync SERVICE {ex.Message}");
                throw new Exception($"DeleteCameraAsync unsuccessful. Error: {ex.Message}");
            }

        }

        public async Task EditCameraAsync(CameraDTO camera)
        {
            try
            {
                var entity = _mapper.Map<CameraDTO, Camera>(camera);
                await _repository.UpdateAsync(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR UPDATE CAMERA WHERE Id = {camera.Id} in EditCameraAsync SERVICE {ex.Message}");
                throw new Exception($"EditCameraAsync unsuccessful. Error: {ex.Message}");
            }

        }

        public async Task<CameraDTO> GetCameraByIdAsync(int cameraId)
        {
            try
            {
                var entity = await _repository.GetAsync(cameraId);
                var dto = _mapper.Map<Camera, CameraDTO>(entity);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT WHERE ID = {cameraId} CAMERA in GetCameraByIdAsync SERVICE {ex.Message}");
                throw new Exception($"GetCameraByIdAsync unsuccessful. Error: {ex.Message}");
            }

        }

        public async Task<List<CameraDTO>> GetCamerasAsync()
        {
            try
            {
                var entities = await _repository.GetAll().ToListAsync();
                var dtos = _mapper.Map<List<Camera>, List<CameraDTO>>(entities);
                return dtos;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT CAMERA in GetCamerasAsync SERVICE {ex.Message}");
                throw new Exception($"GetCamerasAsync unsuccessful. Error: {ex.Message}");
            }

        }
        public async Task AddNewCamera()
        {
            try
            {
                await _cameraRepository.AddNewCamera();
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR INSERT in AddNewCamera SERVICE {ex.Message}");
                throw new Exception($"Add New Camera Service unsuccessful. Error: {ex.Message}");
            }

        }

        public async Task DeleteLastRecord()
        {
            try
            {
                await _cameraRepository.DeleteLastRecord();
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DELETE in DeleteLastRecord SERVICE {ex.Message}");
                throw new Exception($"Delete Last Record unsuccessful. Error: {ex.Message}");
            }

        }
    }
}
