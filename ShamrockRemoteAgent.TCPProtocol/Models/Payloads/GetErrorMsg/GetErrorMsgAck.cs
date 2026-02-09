using ShamrockRemoteAgent.TCPProtocol.Interfaces;

namespace ShamrockRemoteAgent.TCPProtocol.Models.Payloads.GetErrorMsg
{
    public class GetErrorMsgAck: Payload
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
            return new GetErrorMsgAck();
        }
    }
}
