using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;
using ShamrockRemoteAgent.TCPProtocol.Helpers;
using ShamrockRemoteAgent.TCPProtocol.Interfaces;

namespace ShamrockRemoteAgent.TCPProtocol.Models.Packets
{
    public class Packet: IPacket
    {
        public UInt32 PacketLength { get; set; }
        public PacketTypeEnum PacketType { get; set; }
        public UInt32 Timestamp { get; set; }
        public UInt32 PacketSeqNum { get; set; } = 0;
        public byte[] PacketPayload { get; set; } = [];

        private bool ValidateSelf()
        {
            if (PacketType == PacketTypeEnum.NOT_INITIALIZED || 
                Timestamp == 0 || 
                PacketSeqNum == 0) 
                return false;

            Int32 totalLength = sizeof(UInt32) +
                sizeof(PacketTypeEnum) +
                sizeof(UInt32) +
                sizeof(UInt32) +
                PacketPayload.Length;

            return PacketLength == totalLength;
        }

        public byte[] Serialize()
        {
            if (!ValidateSelf())
                return [];

            byte[] serialized = new byte[PacketLength];
            Int32 curPos = 0;

            byte[] packetLengthData = TypeConverter.NumberToBytes<UInt32>(PacketLength);
            Array.Copy(packetLengthData, 0, serialized, curPos, packetLengthData.Length);
            curPos += packetLengthData.Length;

            serialized[curPos++] = (byte)PacketType;

            byte[] timestampData = TypeConverter.NumberToBytes<UInt32>(Timestamp);
            Array.Copy(timestampData, 0, serialized, curPos, timestampData.Length);
            curPos += timestampData.Length;

            byte[] packetSeqNumData = TypeConverter.NumberToBytes<UInt32>(PacketSeqNum);
            Array.Copy(packetSeqNumData, 0, serialized, curPos, packetSeqNumData.Length);
            curPos += packetSeqNumData.Length;

            Array.Copy(PacketPayload, 0, serialized, curPos, PacketPayload.Length);

            return serialized;
        }

        private static bool ValidateBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 4)
                return false;

            Int32 packetLength = TypeConverter.BytesToNumber<Int32>(bytes[0..4]);

            return packetLength == bytes.Length;
        }

        public static IPacket? Deserialize(byte[] bytes)
        {
            if (!ValidateBytes(bytes))
                return null;

            Int32 curPos = 0;
            Int32 packetLengthSize = sizeof(UInt32);
            UInt32 packetLength = TypeConverter.BytesToNumber<UInt32>(bytes[curPos..(curPos + packetLengthSize)]);
            curPos += packetLengthSize;

            PacketTypeEnum packetType = (PacketTypeEnum)bytes[curPos++];

            Int32 timestampSize = sizeof(Int32);
            UInt32 timestamp = TypeConverter.BytesToNumber<UInt32>(bytes[curPos..(curPos + timestampSize)]);
            curPos += timestampSize;

            Int32 packetSeqNumSize = sizeof(Int32);
            UInt32 packetSeqNum = TypeConverter.BytesToNumber<UInt32>(bytes[curPos..(curPos + packetSeqNumSize)]);
            curPos += packetSeqNumSize;

            byte[] packetPayload = bytes[curPos..];

            return new Packet
            {
                PacketLength = packetLength,
                PacketType = packetType,
                Timestamp = timestamp,
                PacketSeqNum = packetSeqNum,
                PacketPayload = packetPayload
            };
        }
    }
}
