using AutoMapper;
using Luxottica.ApplicationServices.CameraAssignments;
using Luxottica.ApplicationServices.Shared.Dto.Acknowledgment;
using Luxottica.Core.Entities.Acknowledgments;
using Luxottica.Core.Entities.CameraAssignments;
using Luxottica.Core.Entities.Cameras;
using Luxottica.DataAccess.Repositories;
using Luxottica.DataAccess.Repositories.Acknowledgment;
using Luxottica.DataAccess.Repositories.DivertOutboundLines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.Acknowledgments
{
    public class AcknowledgmentAppService : IAcknowledgmentAppService
    {
        private readonly IRepository<int, Acknowledgment> _repository;
        private readonly AcknowledgmentRepository _AcknowledgmentRepository;
        public AcknowledgmentAppService(IRepository<int, Acknowledgment> repository, ILogger<AcknowledgmentAppService> logger,
            AcknowledgmentRepository acknowledgmentRepository)
        {
            _repository = repository;
            _AcknowledgmentRepository = acknowledgmentRepository;
        }
        public async Task<int> AddAcknowledgmentAsync(AcknowledgmentAddDTO acknowledgmentAddDto)
        {
            try
            {
                var entity = await _AcknowledgmentRepository.AddAsync(acknowledgmentAddDto);
                return entity.Id;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public async Task DeleteAcknowledgmentAsync(int acknowledgmentId)
        {
            try
            {
                await _repository.DeleteAsync(acknowledgmentId);
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task EditAcknowledgmentAsync(int id, AcknowledgmentAddDTO acknowledgmentAddDTO)
        {
            try
            {
                await _AcknowledgmentRepository.UpdateAsync(id,acknowledgmentAddDTO);
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task<Acknowledgment> GetAcknowledgmentByIdAsync(int acknowledgmentId)
        {
            try
            {
                var acknowledgment = await _repository.GetAsync(acknowledgmentId);
                return acknowledgment;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task<List<Acknowledgment>> GetAcknowledgmentsAsync()
        {
            try
            {
                var acknowledgments = await _repository.GetAll().ToListAsync();
                return acknowledgments;
            }
            catch (Exception ex)
            {
                throw;
            }
        }



    }
}
