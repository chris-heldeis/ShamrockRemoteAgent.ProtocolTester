using ShamrockRemoteAgent.TCPProtocol.Interfaces;

namespace ShamrockRemoteAgent.TCPProtocol.Models.Payloads.ClientDisconnect
{
    public class ClientDisconnectAck: Payload
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
            return new ClientDisconnectAck();
        }
    }
}
