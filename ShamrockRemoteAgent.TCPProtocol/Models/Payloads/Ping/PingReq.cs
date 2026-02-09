using ShamrockRemoteAgent.TCPProtocol.Interfaces;

namespace ShamrockRemoteAgent.TCPProtocol.Models.Payloads.Ping
{
    public class PingReq: Payload
    {
        public new IField[] Fields
        {
            get
            {
                return [];
            }
        }

        public static IPayload? Deserialize()
        {
            return new PingReq();
        }
    }
}
