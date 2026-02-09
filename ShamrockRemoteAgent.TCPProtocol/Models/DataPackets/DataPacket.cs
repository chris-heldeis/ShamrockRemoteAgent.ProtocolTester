using ShamrockRemoteAgent.TCPProtocol.Enums.Packets;
using ShamrockRemoteAgent.TCPProtocol.Helpers;
using ShamrockRemoteAgent.TCPProtocol.Interfaces;

namespace ShamrockRemoteAgent.TCPProtocol.Models.DataPackets
{
    public class DataPacket: IDataPacket
    {
        public UInt32 PacketLength { get; set; }
        public DataPacketTypeEnum PacketType { get; set; }
        public byte[] PacketPayload { get; set; } = [];

        private bool ValidateSelf()
        {
            if (PacketType == DataPacketTypeEnum.NOT_INITIALIZED)
                return false;

            Int32 totalLength = sizeof(UInt32) +
                sizeof(DataPacketTypeEnum) +
                PacketPayload.Length;

            return PacketLength == totalLength;
        }

        public byte[] Serialize()
        {
            if (!ValidateSelf())
                return [];

            byte[] serialized = new byte[PacketLength];
            int curPos = 0;

            byte[] packetLengthData = TypeConverter.NumberToBytes<UInt32>(PacketLength);
            Array.Copy(packetLengthData, 0, serialized, curPos, packetLengthData.Length);
            curPos += packetLengthData.Length;

            serialized[curPos++] = (byte)PacketType;

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

        public static IDataPacket? Deserialize(byte[] bytes)
        {
            if (!ValidateBytes(bytes))
                return null;

            Int32 curPos = 0;
            Int32 packetLengthSize = sizeof(UInt32);
            UInt32 packetLength = TypeConverter.BytesToNumber<UInt32>(bytes[curPos..(curPos + packetLengthSize)]);
            curPos += packetLengthSize;

            DataPacketTypeEnum packetType = (DataPacketTypeEnum)bytes[curPos++];

            byte[] packetPayload = bytes[curPos..];

            return new DataPacket
            {
                PacketLength = packetLength,
                PacketType = packetType,
                PacketPayload = packetPayload
            };
        }
    }
}
