using LuxotticaSorting.ApplicationServices.Shared.Dto.RoutingV10S;

namespace LuxotticaSorting.ApplicationServices.RoutingV10
{
    public interface IRoutingAppService
    {
        Task<List<RoutingV10Dto>> GetBoxesInformationRoutingAsync();
        Task<RoutingV10Dto> GetOrdersBoxIdSAPInformation(string boxIdFromSAP, int trackingId);
        
    }
}
