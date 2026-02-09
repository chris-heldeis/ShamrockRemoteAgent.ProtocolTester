using ShamrockRemoteAgent.TCPProtocol.Interfaces;

namespace ShamrockRemoteAgent.TCPProtocol.Models.Payloads.Close
{
    public class CloseReq: Payload
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
            return new CloseReq();
        }
    }
}
