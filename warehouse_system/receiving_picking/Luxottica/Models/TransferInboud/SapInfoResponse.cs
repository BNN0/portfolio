namespace Luxottica.Models.TransferInboud
{
    public class SapInfoResponse
    {
        public string TOTE_ID { get; set; }
        public string V_TOTE_ID { get; set; }
        public int ZWZONE { get; set; }
        public string? ZTIMESTAMP { get; set; }
        public string ERROR { get; set; }
        public string Request_ts { get; set; }
        public string Response_ts { get; set; }
    }
}
