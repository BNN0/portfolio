namespace Luxottica.Models.NewTote
{
    public class Components
    {
        public class SapInfo
        {
            public InboundToteRegResponse InboundToteRegResponse { get; set; }
        }

        public class InboundToteRegResponse
        {
            public string ToteLpn { get; set; }
            public string Virtual_Tote { get; set; }
            public int Zone_Id { get; set; }
            public long Resp_Timestamp { get; set; }
        }
    }
}
