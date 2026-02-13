using ShamrockRemoteAgent.ClientTester.Models;
using System.Buffers.Binary;

namespace ShamrockRemoteAgent.ClientTester.Services;

public static class BrokerProtocol
{
    private static uint _seq = 0;

    public static byte[] Encode(BrokerPacketType type, byte[] payload)
    {
        uint seq = ++_seq;

        // totalLength = 4 + 1 + 4 + payload.length
        uint totalLength = (uint)(4 + 1 + 4 + payload.Length);

        byte[] buffer = new byte[9 + payload.Length];

        BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(0, 4), totalLength);
        buffer[4] = (byte)type;
        BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(5, 4), seq);

        payload.CopyTo(buffer, 9);

        return buffer;
    }
    public static BrokerDecodedPacket Decode(byte[] buffer)
    {
        if (buffer.Length < 9)
            throw new InvalidOperationException("Invalid broker packet length");

        uint totalLength =
            BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(0, 4));

        byte packetType = buffer[4];

        uint seq =
            BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(5, 4));

        byte[] payload = buffer[9..];

        return new BrokerDecodedPacket
        {
            TotalLength = totalLength,
            PacketType = packetType,
            Sequence = seq,
            Payload = payload
        };
    }
}

public enum BrokerPacketType : byte
{
    COM_DATA = 0x01,
    NEED_WAIT = 0x02,
    READY_CLOSE = 0x03,
    CON_TIMEOUT = 0x04,
    DATA_INVALID = 0x05
}

public class BrokerDecodedPacket
{
    public uint TotalLength { get; init; }
    public byte PacketType { get; init; }
    public uint Sequence { get; init; }
    public byte[] Payload { get; init; } = [];
}
