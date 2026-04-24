namespace Luxottica.Models.TransferInboud
{
    public class ToteScanInfoModel
    {
        public string CamId { get; set; }

        public int TrackingId { get; set; }

        public string toteLpn { get; set; }

        public int ScannerNLaneWStatus { get; set; }

        public int ScannerNLaneWFull { get; set; }
    }
}
