using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;

namespace ShamrockRemoteAgent.TCPProtocol.Interfaces
{
    public interface IDataPacket
    {
        UInt32 PacketLength { get; set; }
        DataPacketTypeEnum PacketType { get; set; }
        byte[] PacketPayload { get; set; }
        abstract public byte[] Serialize();
        abstract public static IDataPacket? Deserialize(byte[] data);
    }
}
