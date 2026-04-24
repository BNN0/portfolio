using AutoMapper;
using Luxottica.ApplicationServices.Shared.Dto.CameraAssignments;
using Luxottica.Core.Entities.CameraAssignments;
using Luxottica.DataAccess;
using Luxottica.DataAccess.Repositories;
using Luxottica.DataAccess.Repositories.CameraAssignments;
using Luxottica.DataAccess.Repositories.ToteInformation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Luxottica.ApplicationServices.CameraAssignments
{
    public class CameraAssignmentService : ICameraAssignmentService
    {
        private readonly V10Context _context;
        private readonly IRepository<int, CameraAssignment> _repository;
        private readonly IMapper _mapper;
        private readonly CameraAssignmentRepository _cameraAssignmentRepository;
        private readonly ILogger<CameraAssignmentService> _logger;

        public CameraAssignmentService(V10Context context, IRepository<int, CameraAssignment> repository, CameraAssignmentRepository cameraAssignmentRepository, IMapper mapper, ILogger<CameraAssignmentService> logger)
        {
            _context = context;
            _repository = repository;
            _mapper = mapper;
            _cameraAssignmentRepository = cameraAssignmentRepository;
            _logger = logger;
        }
        public async Task AddCameraAssignmentAsync(CameraAssignmentAddDto cameraAssignment)
        {
            try
            {
                var m = _mapper.Map<CameraAssignment>(cameraAssignment);
                await _repository.AddAsync(m);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR INSERT CameraAssignment in AddCameraAssignmentAsync Service Message {ex.Message}");
                throw new Exception($"An error has occurred: {ex.Message}");
            }
        }

        public async Task<bool> CameraAssignmentExistsAsync(int divertLineId)
        {
            var existingAssignment = await _context.CameraAssignments
                .AnyAsync(ca => ca.DivertLineId == divertLineId);

            return existingAssignment;
        }

        public async Task<bool> CameraAssignmentExistsEditAsync(int divertLineId, int assigmentId)
        {
            var existingAssignment = await _context.CameraAssignments
                .AnyAsync(ca => ca.DivertLineId == divertLineId && ca.Id != assigmentId);

            return existingAssignment;
        }


        public async Task DeleteCameraAssignmentAsync(int cameraAssignmentId)
        {
            try
            {
                await _repository.DeleteAsync(cameraAssignmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR DELETE CameraAssignment Where Id = {cameraAssignmentId} in DeleteCameraAssignmentAsync Service Message {ex.Message}");
                throw new Exception($"An error has occurred: {ex.Message}");
            }

        }

        public async Task EditCameraAssignmentAsync(int id, CameraAssignmentAddDto cameraAssignment)
        {
            try
            {
                var m = _mapper.Map<CameraAssignment>(cameraAssignment);
                m.Id = id;
                await _repository.UpdateAsync(m);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR Update CameraAssignment Where Id = {id} in EditCameraAssignmentAsync Service Message {ex.Message}");
                throw new Exception($"An error has occurred: {ex.Message}");
            }
        }

        public async Task<List<CameraAssignmentDto>> GetCameraAssignmentAsync()
        {
            try
            {
                var m = await _repository.GetAll().ToListAsync();
                List<CameraAssignmentDto> cameraAssignment = _mapper.Map<List<CameraAssignmentDto>>(m);
                return cameraAssignment;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT FROM CameraAssignment in GetCameraAssignmentAsync Service Message {ex.Message}");
                throw new Exception($"An error has occurred: {ex.Message}");
            }
        }

        public async Task<CameraAssignmentDto> GetCameraAssignmentByIdAsync(int cameraAssignmentId)
        {
            try
            {
                var m = await _repository.GetAsync(cameraAssignmentId);
                CameraAssignmentDto dto = _mapper.Map<CameraAssignmentDto>(m);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT FROM CameraAssignment WHERE Id = {cameraAssignmentId} in GetCameraAssignmentAsync Service Message {ex.Message}");
                throw new Exception($"An error has occurred: {ex.Message}");
            }
        }

        public async Task<List<CameraAssignmentGetAllDto>> GetAllExtraAsync()
        {
            try
            {
                var result = await _cameraAssignmentRepository.GetAllExtraAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT FROM CameraAssignment in GetAllExtraAsync Service Message {ex.Message}");
                throw new Exception($"An error has occurred: {ex.Message}");
            }
        }
    }
}

