using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;

namespace ShamrockRemoteAgent.ClientTester.Models
{
    public class PayloadMenuItem
    {
        public DataPacketTypeEnum Id { get; set; }
        public string PayloadName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string PageName { get; set; } = string.Empty;
    }
}
