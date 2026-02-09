using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;

namespace ShamrockRemoteAgent.MasterTester.Models
{
    public class DecodedPacket
    {
        public byte[] RawBytes { get; set; } = [];
        public DataPacketTypeEnum PacketType { get; set; }
        public uint PacketLength { get; set; }

        public object? Payload { get; set; }   // decoded payload object
        public string PayloadDetails { get; set; } = "";
    }
}
