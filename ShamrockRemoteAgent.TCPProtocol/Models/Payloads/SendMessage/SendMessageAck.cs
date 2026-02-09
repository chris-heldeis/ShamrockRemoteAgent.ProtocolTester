using ShamrockRemoteAgent.TCPProtocol.Interfaces;

namespace ShamrockRemoteAgent.TCPProtocol.Models.Payloads.SendMessage
{
    public class SendMessageAck: Payload
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
            return new SendMessageAck();
        }
    }
}
