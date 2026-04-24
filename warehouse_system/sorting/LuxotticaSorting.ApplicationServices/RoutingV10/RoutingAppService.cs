using AutoMapper;
using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.Shared.Dto.RoutingV10S;
using LuxotticaSorting.Core.WCSRoutingV10;
using LuxotticaSorting.DataAccess.Repositories.RoutingV10S;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LuxotticaSorting.ApplicationServices.RoutingV10
{
    public class RoutingAppService : IRoutingAppService
    {
        private readonly IRepository<int, WCSRoutingV10> _repository;
        private readonly RoutingV10Repository _routingv10Repository;
        private readonly IMapper _mapper;
        private readonly ILogger<RoutingAppService> _logger;
        public RoutingAppService(IRepository<int, WCSRoutingV10> repository, RoutingV10Repository routingv10Repository, IMapper mapper, ILogger<RoutingAppService> logger)
        {
            _repository = repository;
            _routingv10Repository = routingv10Repository;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<List<RoutingV10Dto>> GetBoxesInformationRoutingAsync()
        {
            try
            {
                var b = await _repository.GetAll().ToListAsync();
                List<RoutingV10Dto> boxesInformation = _mapper.Map<List<RoutingV10Dto>>(b);
                return boxesInformation;
            }
            catch (Exception ex)
            {
                _logger.LogError($"RoutingAppService method GetBoxesInformationRoutingAsync failed, error: {ex.Message}");
                throw;
            }
        }



        public async Task<RoutingV10Dto> GetOrdersBoxIdSAPInformation(string boxIdFromSAP, int trackingId)
        {
            try
            {
                var b = await _routingv10Repository.GetByBoxId(boxIdFromSAP, trackingId);
                RoutingV10Dto dto = _mapper.Map<RoutingV10Dto>(b);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"RoutingAppService method GetBoxesInformationRoutingAsync failed, error: {ex.Message}");
                throw;
            }
        }
    }
}
