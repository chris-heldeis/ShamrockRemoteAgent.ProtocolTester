using ShamrockRemoteAgent.TCPProtocol.Interfaces;

namespace ShamrockRemoteAgent.TCPProtocol.Models.Payloads.Ping
{
    public class PingRes: Payload
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
            return new PingRes();
        }
    }
}
