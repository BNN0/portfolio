using LuxotticaSorting.ApplicationServices.Shared.DTO.TrafficLights;
using LuxotticaSorting.ApplicationServices.TrafficLights;
using LuxotticaSorting.DataAccess;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OpenQA.Selenium.Support.UI;
using System.Net.WebSockets;
using System.Text;

namespace LuxotticaSorting
{
    public class WebSocketHandler
    {
        private List<WebSocket> webSockets = new List<WebSocket>();

        TrafficLightDataDto data = new TrafficLightDataDto();
        TrafficLightDataDto data2 = new TrafficLightDataDto();
        InformationGeneralTruckLines informationLine2 = new InformationGeneralTruckLines();
        InformationGeneralTruckLines informationLine4 = new InformationGeneralTruckLines();


        public readonly SortingContext _context;
        private readonly ITrafficLightAppService _trafficService;
        public WebSocketHandler(SortingContext context, ITrafficLightAppService trafficService)
        {
            _context = context;
            _trafficService = trafficService;
        }

        public async Task HandleWebSocketAsync(WebSocket webSocket)
        {
            webSockets.Add(webSocket);

            try
            {
                var previoustrafficLightForLine2 = await StatusTrafficLigthTruckLine2();
                data = previoustrafficLightForLine2;
                var previoustrafficLightForLine4 = await StatusTrafficLigthTruckLine4();
                data2 = previoustrafficLightForLine4;

                informationLine2.Route = "/statusLine2";
                informationLine2.Data = data;

                informationLine4.Route = "/statusLine4";
                informationLine4.Data = data2;

                var combinedResultsPrevious = new[] { informationLine2, informationLine4 };
                var jsonPrevious = JsonConvert.SerializeObject(combinedResultsPrevious);
                var bufferPrevious = Encoding.UTF8.GetBytes(jsonPrevious);

                await webSocket.SendAsync(new ArraySegment<byte>(bufferPrevious), WebSocketMessageType.Text, true, CancellationToken.None);

                while (webSocket.State == WebSocketState.Open)
                {
                    var currentTrafficLightForLine2 = await StatusTrafficLigthTruckLine2();
                    var currentTrafficLightForLine4 = await StatusTrafficLigthTruckLine4();

                    if (!string.Equals(JsonConvert.SerializeObject(data), JsonConvert.SerializeObject(currentTrafficLightForLine2)) ||
                        !string.Equals(JsonConvert.SerializeObject(data2), JsonConvert.SerializeObject(currentTrafficLightForLine4)))
                    {
                        informationLine2.Route = "/statusLine2";
                        informationLine2.Data = currentTrafficLightForLine2;

                        informationLine4.Route = "/statusLine4";
                        informationLine4.Data = currentTrafficLightForLine4;

                        data = currentTrafficLightForLine2;
                        data2 = currentTrafficLightForLine4;
                        var combinedResults = new[] { informationLine2, informationLine4 };
                        var json = JsonConvert.SerializeObject(combinedResults);
                        var buffer = Encoding.UTF8.GetBytes(json);

                        await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error has occurred.");
            }
            finally
            {
                webSockets.Remove(webSocket);
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
            }
        }
        
        private async Task<TrafficLightDataDto> StatusTrafficLigthTruckLine2()
        {
            try
            {
                var result = await _trafficService.GetStatusLightLine2();
                return result;
            }
            catch (Exception)
            {
                throw new Exception("An internal error has occurred.");
            }
        }

        private async Task<TrafficLightDataDto> StatusTrafficLigthTruckLine4()
        {
            try
            {
                var result = await _trafficService.GetStatusLightLine4();
                return result;
            }
            catch (Exception)
            {
                throw new Exception("An internal error has occurred.");
            }
        }
        
        private class InformationGeneralTruckLines
        {
            public string Route { get; set; }
            public TrafficLightDataDto Data { get; set; }
        }        
    }
}
