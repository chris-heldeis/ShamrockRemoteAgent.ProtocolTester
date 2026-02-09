using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;

namespace ShamrockRemoteAgent.TCPProtocol.Interfaces
{
    public interface IPacket
    {
        UInt32 PacketLength { get; set; }
        PacketTypeEnum PacketType { get; set; }
        UInt32 Timestamp { get; set; }
        UInt32 PacketSeqNum { get; set; }
        byte[] PacketPayload { get; set; }
        abstract public byte[] Serialize();
        abstract public static IPacket? Deserialize(byte[] data);
    }
}
