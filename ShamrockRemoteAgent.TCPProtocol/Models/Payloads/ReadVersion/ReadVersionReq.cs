using ShamrockRemoteAgent.TCPProtocol.Interfaces;

namespace ShamrockRemoteAgent.TCPProtocol.Models.Payloads.ReadVersion
{
    public class ReadVersionReq: Payload
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
            return new ReadVersionReq();
        }
    }
}
