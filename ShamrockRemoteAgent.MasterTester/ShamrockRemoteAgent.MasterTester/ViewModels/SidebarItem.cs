using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;

namespace ShamrockRemoteAgent.MasterTester.ViewModels
{
    public class SidebarItem
    {
        public DataPacketTypeEnum Id { get; set; }
        public string PayloadName { get; set; } = "";
        public string Title { get; set; } = "";
        public string PageName { get; set; } = "";
    }
}
