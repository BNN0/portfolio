namespace Luxottica.Models.Tote
{
    public class BorderLineModel
    {
        public string CamId { get; set; }

        public string ToteLPN { get; set; }

        public int TrackingId {  get; set; }

        public int scanner_N_lane_w_status { get; set; }
        public int scanner_N_lane_w_full { get; set; }
    }
}
